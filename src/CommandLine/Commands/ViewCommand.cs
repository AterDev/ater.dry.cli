namespace Droplet.CommandLine.Commands;

public class ViewCommand : CommandBase
{
    public string Type { get; set; } = "angular";
    public string EntityPath { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string DtoPath { get; set; } = default!;
    public string OutputPath { get; set; } = default!;

    public NgPageGenerate Gen { get; set; }

    public ViewCommand(string entityPath, string dtoPath, string outputPath)
    {
        EntityPath = entityPath;
        DtoPath = dtoPath;
        OutputPath = outputPath;
        Instructions.Add($"  🔹 generate module,routing and menu.");
        Instructions.Add($"  🔹 generate pages.");

        if (!File.Exists(entityPath))
        {
            throw new FileNotFoundException();
        }
        EntityName = Path.GetFileNameWithoutExtension(entityPath);
        Gen = new NgPageGenerate(EntityName, dtoPath, outputPath);
    }
    public async Task RunAsync()
    {
        Console.WriteLine(Instructions[0]);
        GenerateMenu();
        await GenerateModuleWithRoutingAsync();
        Console.WriteLine(Instructions[1]);
        await GeneratePagesAsync();
        Console.WriteLine("😀 View generate completed!" + Environment.NewLine);
    }

    public void GenerateMenu()
    {

    }

    public async Task GenerateModuleWithRoutingAsync()
    {
        var moduleName = EntityName.ToHyphen();
        var dir = Path.Combine(OutputPath, "src", "app", "pages", moduleName);
        var module = Gen.GetModule();
        var routing = Gen.GetRoutingModule();
        var moduleFilename = moduleName + ".module.ts";
        var routingFilename = moduleName + "-routing.module.ts";
        await GenerateFileAsync(dir, moduleFilename, module);
        await GenerateFileAsync(dir, routingFilename, routing);

    }

    /// <summary>
    /// 生成实体的列表、添加等页面
    /// </summary>
    /// <returns></returns>
    public async Task GeneratePagesAsync()
    {
        var moduleName = EntityName.ToHyphen();
        var dir = Path.Combine(OutputPath, "src", "app", "pages", moduleName);

        var addComponent = Gen.BuildAddPage();
        var editComponent = Gen.BuildEditPage();
        var indexComponent = Gen.BuildIndexPage();
        var detailComponent = Gen.BuildDetailPage();
        var layoutComponent = Gen.BuildLayout();
        var confirmDialogComponent = Gen.BuildConfirmDialog();

        var componentsModule = Gen.GetComponentModule();

        await GenerateComponentAsync(dir, addComponent);
        await GenerateComponentAsync(dir, editComponent);
        await GenerateComponentAsync(dir, detailComponent);
        await GenerateComponentAsync(dir, indexComponent);

        dir = Path.Combine(OutputPath, "src", "app", "components");
        await GenerateComponentAsync(dir, layoutComponent);
        await GenerateComponentAsync(dir, confirmDialogComponent);
        await GenerateFileAsync(dir, "components.module.ts", componentsModule);
    }

    /// <summary>
    /// 生成组件文件
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public async Task GenerateComponentAsync(string dir, NgComponentInfo info)
    {
        var path = Path.Combine(dir, info.Name);
        await GenerateFileAsync(path, info.Name + ".component.ts", info.TsContent!);
        await GenerateFileAsync(path, info.Name + ".component.css", info.CssContent!);
        await GenerateFileAsync(path, info.Name + ".component.html", info.HtmlContent!);
    }
}
