﻿using Share.Models.GenActionDtos;
using Share.Models.GenStepDtos;

namespace Application.Managers;
/// <summary>
/// The project's generate action
/// </summary>
public class GenActionManager(
    DataAccessContext<GenAction> dataContext,
    CodeGenService codeGenService,
    ILogger<GenActionManager> logger,
    IProjectContext projectContext,
    IUserContext userContext) : ManagerBase<GenAction>(dataContext, logger)
{
    private readonly IUserContext _userContext = userContext;
    private readonly IProjectContext _projectContext = projectContext;
    private readonly CodeGenService _codeGen = codeGenService;


    /// <summary>
    /// 添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> CreateNewEntityAsync(GenActionAddDto dto)
    {
        var entity = dto.MapTo<GenActionAddDto, GenAction>();
        entity.ProjectId = _projectContext.ProjectId;
        return await AddAsync(entity) ? entity.Id : null;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(GenAction entity, GenActionUpdateDto dto)
    {
        entity.Merge(dto);
        // TODO:完善更新逻辑
        return await UpdateAsync(entity);
    }

    public async Task<PageList<GenActionItemDto>> ToPageAsync(GenActionFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.Name, q => q.Name.ToLower().Contains(filter.Name!.Trim().ToLower()))
            .WhereNotNull(filter.SourceType, q => q.SourceType == filter.SourceType)
            .WhereNotNull(filter.ProjectId, q => q.ProjectId == filter.ProjectId);

        return await ToPageAsync<GenActionFilterDto, GenActionItemDto>(filter);
    }

    /// <summary>
    /// 获取步骤
    /// </summary>
    /// <param name="actionId"></param>
    /// <returns></returns>
    public async Task<List<GenStepItemDto>> GetStepsAsync(Guid actionId)
    {
        var data = await Query.Where(q => q.Id == actionId)
             .SelectMany(q => q.GenSteps)
             .ProjectTo<GenStepItemDto>()
             .ToListAsync();
        return data;
    }

    /// <summary>
    /// 获取实体详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenActionDetailDto?> GetDetailAsync(Guid id)
    {
        return await FindAsync<GenActionDetailDto>(e => e.Id == id);
    }

    /// <summary>
    /// 唯一性判断
    /// </summary>
    /// <param name="name">唯一标识</param>
    /// <param name="id">排除当前</param>
    /// <returns></returns>
    public async Task<bool> IsUniqueAsync(string name, Guid? id = null)
    {
        // 自定义唯一性验证参数和逻辑
        return await Command.Where(q => q.Name == name)
            .WhereNotNull(id, q => q.Id != id)
            .AnyAsync();
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public new async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        return await base.DeleteAsync(ids, softDelete);
    }

    /// <summary>
    /// 数据权限验证
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<GenAction?> GetOwnedAsync(Guid id)
    {
        var query = Command.Where(q => q.Id == id);
        // TODO:自定义数据权限验证
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 添加步骤
    /// </summary>
    /// <param name="id"></param>
    /// <param name="stepIds"></param>
    /// <returns></returns>
    public async Task<bool> AddStepsAsync(Guid id, List<Guid> stepIds)
    {
        await Database.BeginTransactionAsync();
        try
        {
            await CommandContext.GenActionGenSteps.Where(q => q.GenActionsId == id)
                .ExecuteDeleteAsync();

            var actionSteps = stepIds.Select(q => new GenActionGenStep
            {
                GenActionsId = id,
                GenStepsId = q
            });
            await CommandContext.GenActionGenSteps.AddRangeAsync(actionSteps);
            await SaveChangesAsync();
            await Database.CommitTransactionAsync();
            return true;
        }
        catch (Exception ex)
        {
            await Database.RollbackTransactionAsync();
            _logger.LogError(ex, "Add steps failed");
            return false;
        }
    }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> ExecuteActionAsync(Guid id)
    {
        var action = await Command.Where(a => a.Id == id)
            .Include(a => a.GenSteps)
            .FirstAsync();
        action.ActionStatus = ActionStatus.InProgress;
        var variables = action.Variables;
        if (action.GenSteps.Count > 0)
        {
            try
            {
                foreach (var step in action.GenSteps)
                {
                    switch (step.GenStepType)
                    {
                        case GenStepType.File:
                            if (step.Path.NotEmpty() && File.Exists(step.Path))
                            {
                                step.Content = File.ReadAllText(step.Path);
                            }
                            step.OutputContent = _codeGen.GenTemplateFile(step.Content ?? "", variables);
                            if (step.OutputPath.NotEmpty())
                            {
                                // 处理outputPath中的变化
                                var outputPath = step.OutputPathFormat(variables);
                                outputPath = Path.Combine(_projectContext.SolutionPath!, outputPath);
                                File.WriteAllText(outputPath, step.OutputContent);
                                return true;
                            }
                            break;
                        case GenStepType.Command:
                            break;
                        case GenStepType.Script:

                            break;
                        default:
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                action.ActionStatus = ActionStatus.Failed;
                _logger.LogError(ex, "Execute action failed");
                return false;
            }
        }

        action.ActionStatus = ActionStatus.Success;
        return await SaveChangesAsync() > 0;
    }
}