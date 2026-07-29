using CoreMod.Services;
using Microsoft.Extensions.DependencyInjection;
using Share;
using Share.Entity;
using Share.Models;
using Xunit;

namespace CoreMod.Tests.Services;

public class ActionRunModelServiceTests
{
    [Fact]
    public void Create_ShouldMergeAndDistinctVariablesByKey()
    {
        // Arrange
        var projectContext = new SolutionContext(new ServiceCollection().BuildServiceProvider());
        var service = new ActionRunModelService(projectContext);

        var source = new List<Variable>
        {
            new() { Key = "K1", Value = "V1" },
            new() { Key = "K2", Value = "V2" },
        };

        var extra = new List<Variable>
        {
            new() { Key = "K2", Value = "V2-override" },
            new() { Key = "K3", Value = "V3" },
        };

        // Act
        var model = service.Create(source, extra);

        // Assert
        Assert.Equal(3, model.Variables.Count);
        Assert.Contains(model.Variables, v => v.Key == "K1" && v.Value == "V1");
        Assert.Contains(model.Variables, v => v.Key == "K2" && v.Value == "V2");
        Assert.Contains(model.Variables, v => v.Key == "K3" && v.Value == "V3");
    }

    [Fact]
    public void ApplyModelInfo_ShouldSetModelPropertiesAndStandardVariables()
    {
        // Arrange
        var projectContext = new SolutionContext(new ServiceCollection().BuildServiceProvider());
        var service = new ActionRunModelService(projectContext);
        var model = service.Create(
            [new Variable { Key = "ModelName", Value = "OldName" }, new Variable { Key = "X", Value = "1" }],
            null
        );

        var modelInfo = new TypeMeta
        {
            Name = "OrderItem",
            Comment = "Comment",
            CommentSummary = "Summary",
            PropertyInfos =
            [
                new PropertyInfo
                {
                    Name = "Name",
                    Type = "string",
                }
            ]
        };

        // Act
        service.ApplyModelInfo(model, modelInfo);

        // Assert
        Assert.Equal("OrderItem", model.ModelName);
        Assert.Equal("Summary", model.Description);
        Assert.Single(model.PropertyInfos);

        var nameVars = model.Variables.Where(v => v.Key == "ModelName").ToList();
        Assert.Single(nameVars);
        Assert.Equal("OrderItem", nameVars[0].Value);

        var hyphenVars = model.Variables.Where(v => v.Key == "ModelNameHyphen").ToList();
        Assert.Single(hyphenVars);
        Assert.Equal("order-item", hyphenVars[0].Value);

        Assert.Contains(model.Variables, v => v.Key == "X" && v.Value == "1");
    }
}
