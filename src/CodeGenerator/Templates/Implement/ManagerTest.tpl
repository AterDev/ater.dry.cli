using Application.IManager;
using Core.Entities.${Namespace};
using Core.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Share.Models.${EntityName}Dtos;

namespace Application.Test.Managers;

public class ${EntityName}ManagerTest : BaseTest
{
    private readonly I${EntityName}Manager manager;

    public ${EntityName}ManagerTest(WebApplicationFactory<Program> factory) : base(factory)
    {
        manager = Services.GetRequiredService<I${EntityName}Manager>();
    }


    [Fact]
    public async Task Shoud_AddAsync()
    {
        // var entity = new ${EntityName}{};
        // var res = await manager.AddAsync(entity);
        // Assert.Equal(entity.UserName, res.UserName);
    }


    [Fact]
    public async Task Should_UpdateAsync()
    {
        var dto = new ${EntityName}UpdateDto();
        var entity = manager.Command.Db.FirstOrDefault();

        if (entity != null)
        {
            // dto.UserName = "updateUser";
            var res = await manager.UpdateAsync(entity, dto);
            // Assert.Equal(dto.UserName, res.UserName);
        }
    }

    [Fact]
    public async Task Should_QueryAsync()
    {
        var filter = new ${EntityName}FilterDto()
        {
            PageIndex = 1,
            PageSize = 2
        };
        var res = await manager.FilterAsync(filter);
        Assert.True(res != null && res.Count != 0 && res.Data.Count <= 2);
    }
}
