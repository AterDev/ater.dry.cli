namespace Droplet.CommandLine.Commands;

public class ViewCommand : CommandBase
{
    public string Type { get; set; } = "angular";
    public string EntityPath { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string DtoPath { get; set; } = default!;
    public string OutputPath { get; set; } = default!;
    public string? ModuleName { get; set; }
    public string? Route { get; set; }
    public string? EntityComment { get; set; }
    /// <summary>
    /// 模块与子模块路由map
    /// </summary>
    public List<KeyValuePair<string, string>> ModuleRouteMap { get; } = new();
    public List<KeyValuePair<string, string>> RouteNameMap { get; } = new();

    public NgPageGenerate Gen { get; set; }

    public ViewCommand(string dtoPath, string outputPath)
    {
        DtoPath = dtoPath;
        OutputPath = outputPath;
        Instructions.Add($"  🔹 generate module,routing and menu.");
        Instructions.Add($"  🔹 generate pages.");
        Gen = new NgPageGenerate(EntityName, dtoPath, outputPath);
    }


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

    public void SetEntityPath(string entityPath)
    {
        EntityPath = entityPath;

        if (!File.Exists(entityPath))
        {
            throw new FileNotFoundException();
        }
        EntityName = Path.GetFileNameWithoutExtension(entityPath);
        Gen = new NgPageGenerate(EntityName, DtoPath, OutputPath);
    }

    public async Task RunAsync()
    {
        Console.WriteLine(Instructions[0]);
        await GenerateModuleWithRoutingAsync();
        Console.WriteLine(Instructions[1]);
        await GeneratePagesAsync();
        Console.WriteLine("😀 View generate completed!" + Environment.NewLine);
    }

    /// <summary>
    /// 生成模块路由
    /// </summary>
    public async Task GenerateModuleRouteAsync()
    {
        // 按模块分组
        var list = ModuleRouteMap.GroupBy(g=>g.Key)
            .Select(g=>new
            {
                module = g.Key,
                route = g.Select(g=>g.Value).ToList()
            }).ToList();

        foreach (var item in list)
        {
            var moduleContent = NgPageGenerate.GetGroupModule(item.module.ToPascalCase(), item.route);
            var moduleRoutingContent = NgPageGenerate.GetGroupRoutingModule(item.module.ToPascalCase());

            var moduleFilename = item.module.ToHyphen() + ".module.ts";
            var routingFilename = item.module.ToHyphen() + "-routing.module.ts";
            var dir = Path.Combine(OutputPath, "src", "app", "pages", item.module.ToHyphen());
            await GenerateFileAsync(dir, moduleFilename, moduleContent, true);
            await GenerateFileAsync(dir, routingFilename, moduleRoutingContent, true);
        }
    }
    /// <summary>
    /// 更新导航菜单
    /// </summary>
    public async Task UpdateMenus()
    {
        // replace <!-- {Menus} -->
        var dir = Path.Combine(OutputPath, "src", "app", "components", "navigation");
        var fileName = "navigation.component.html";
        var content = await File.ReadAllTextAsync(Path.Combine(dir,fileName));


        var list = ModuleRouteMap.GroupBy(g=>g.Key)
            .Select(g=>new
            {
                module = g.Key,
                route = g.Select(g=>g.Value).ToList(),
            }).ToList();

        var menusContent = "";
        foreach (var item in list)
        {
            menusContent += NgPageGenerate.GetNavigation(item.module.ToPascalCase(), item.route, RouteNameMap);
        }
        // 插入的起始点
        var startTmp = "<ng-template #automenu>";
        var endTmp  = "</ng-template>";
        var startIndex =  content.LastIndexOf(startTmp)+startTmp.Length;
        var endIndex = content.LastIndexOf(endTmp);
        var sb = new StringBuilder();
        sb.Append(content.AsSpan(0, startIndex));
        sb.AppendLine();
        sb.Append(menusContent);
        sb.Append(content.AsSpan(endIndex));
        await GenerateFileAsync(dir, fileName, sb.ToString(), true);
    }


    private async Task GenerateModuleWithRoutingAsync()
    {
        var entityName = EntityName.ToHyphen();
        var moduleName = ModuleName ?? EntityName;
        var routeName = Route?.ToPascalCase().ToHyphen();
        var dir = Path.Combine(OutputPath, "src", "app", "pages", moduleName, routeName??"");

        var module = Gen.GetModule(Route?.ToPascalCase());
        var routing = Gen.GetRoutingModule(moduleName, Route?.ToPascalCase());
        var moduleFilename =  routeName + ".module.ts";
        var routingFilename = routeName + "-routing.module.ts";
        await GenerateFileAsync(dir, moduleFilename, module);
        await GenerateFileAsync(dir, routingFilename, routing);

        if (!string.IsNullOrWhiteSpace(Route))
        {
            ModuleRouteMap.Add(new KeyValuePair<string, string>(moduleName, Route.ToPascalCase()));
            RouteNameMap.Add(new KeyValuePair<string, string>(Route.ToPascalCase(), EntityComment ?? ""));
        }
    }

    /// <summary>
    /// 生成实体的列表、添加等页面
    /// </summary>
    /// <returns></returns>
    private async Task GeneratePagesAsync()
    {
        var moduleName = ModuleName ?? EntityName;
        var dir = Path.Combine(OutputPath, "src", "app", "pages", moduleName, Route?.ToPascalCase().ToHyphen()??"");

        var addComponent = Gen.BuildAddPage();
        var editComponent = Gen.BuildEditPage();
        var indexComponent = Gen.BuildIndexPage();
        var detailComponent = Gen.BuildDetailPage();
        var layoutComponent = Gen.BuildLayout();
        var confirmDialogComponent = NgPageGenerate.BuildConfirmDialog();
        var componentsModule = NgPageGenerate.GetComponentModule();

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
