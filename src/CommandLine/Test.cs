using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using CodeGenerator.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Droplet.CommandLine;

public class Test
{
    /// <summary>
    /// dto 生成测试
    /// </summary>
    /// <param name="entityPath"></param>
    public void TestDtoGen(string entityPath, string dtoPath)
    {
        var gen = new DtoGenerate(entityPath, dtoPath);
        gen.GenerateDtos();
    }


    public async Task TestNgGenAsync(string url, string output)
    {
        var cmd = new RootCommands();
        await cmd.GenerateNgAsync(url, output);
    }

    public void TestEFCoreModel()
    {
        var builder = new Model().Builder;
        builder.Entity(typeof(Comments), ConfigurationSource.Explicit);

        var entity = builder.Metadata.FindEntityType(typeof(Comments));
        var contentProperty = entity.FindProperty(nameof(Comments.Content));

    }

    public void TestEFCoreEntityTypeModel()
    {

    }
}
