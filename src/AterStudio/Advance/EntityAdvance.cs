using System.Reflection;
using AterStudio.Manager;
using Core.Entities;
using Core.Infrastructure;
using Core.Infrastructure.Helper;
using Datastore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AterStudio.Advance;

public class EntityAdvance
{

    private readonly DbContext _dbContext;
    public EntityAdvance(DbContext dbContext)
    {
        _dbContext = dbContext;
    }


    /// <summary>
    /// 获取实体表结构
    /// </summary>
    /// <param name="id">项目id</param>
    /// <returns>markdown format</returns>
    public string GetDatabaseStructure(Guid id)
    {
        var project = _dbContext.Projects.FindById(id);

        var projectPath = ProjectManager.GetProjectRootPath(project.Path);
        // get contextbase path 
        var contextPath = Directory.GetFiles(projectPath, "ContextBase.cs", SearchOption.AllDirectories)
            .FirstOrDefault();
        if (contextPath != null)
        {
            // parse context content and get all proporties 
            var compilation = new CompilationHelper(project.EntityFrameworkPath);
            compilation.AddSyntaxTree(File.ReadAllText(contextPath));

            var propertyTypes = compilation.GetPropertyTypes();

            // search entity use propertyType and parse every entity use CompilationHelper
            foreach (var propertyType in propertyTypes)
            {
                var entityPath = Directory.GetFiles(project.EntityPath, $"{propertyType}.cs", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (entityPath != null)
                {
                    compilation.AddSyntaxTree(File.ReadAllText(entityPath));

                }
            }
        }

        return default;
    }
    //public EntityInfo ParseEntity(ClassDeclarationSyntax classDeclaration)
    //{
    //    string name = classDeclaration!.Identifier.ToString();
    //    string comment = GetClassComment(classDeclaration);
    //    string? namespaceName = CompilationHelper.GetNamesapce();

    //    return new EntityInfo()
    //    {
    //        Name = name,
    //        ProjectId = Const.PROJECT_ID,
    //        NamespaceName = namespaceName,
    //        Comment = comment,
    //        PropertyInfos = GetPropertyInfos(),
    //    };
    //    return default;
    //}
}
