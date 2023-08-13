using ${Namespace}.IManager;
using ${EntityNamespace};
using Ater.Web.Core.Utils;
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
    public async Task ${EntityName}ShouldPass()
    {
        await Should_AddAsync();
        await Should_UpdateAsync();
        await Should_QueryAsync();
    }

    async internal Task Should_AddAsync()
    {
${AddContent}
    }

    async internal Task Should_UpdateAsync()
    {
${UpdateContent}
    }

    async internal Task Should_QueryAsync()
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
