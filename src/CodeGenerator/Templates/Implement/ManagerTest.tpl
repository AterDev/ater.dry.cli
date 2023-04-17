using ${Namespace}.IManager;
using ${EntityNamespace};
using Core.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Share.Models.${EntityName}Dtos;

namespace Application.Test.Managers;

public class ${EntityName}ManagerTest : BaseTest
{
    private readonly I${EntityName}Manager manager;
    public string RandomString { get; set; }

    public ${EntityName}ManagerTest(WebApplicationFactory<Program> factory) : base(factory)
    {
        manager = Services.GetRequiredService<I${EntityName}Manager>();
        RandomString = DateTime.Now.ToString("MMddmmss");
    }

    [Fact]
    public async Task Shoud_AddAsync()
    {
${AddContent}
    }

    [Fact]
    public async Task Should_UpdateAsync()
    {
${UpdateContent}
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
