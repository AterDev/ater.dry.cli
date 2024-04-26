namespace Command.Share.Commands;

public class ViewCommand : CommandBase
{
    public string Type { get; set; } = "angular";
    public string EntityFilePath { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string DtoPath { get; set; } = default!;
    public string OutputPath { get; set; } = default!;
    public string? ModuleName { get; set; }
    public string? Route { get; set; }
    public string? EntityComment { get; set; }
    public string SolutionPath { get; }
    public NgPageGenerate? Gen { get; set; }
    /// <summary>
    /// 模块与子模块路由map
    /// </summary>
    public List<KeyValuePair<string, string>> ModuleRouteMap { get; } = [];
    public List<KeyValuePair<string, string>> RouteNameMap { get; } = [];


    public ViewCommand(string dtoPath, string outputPath)
    {
        DtoPath = dtoPath;
        OutputPath = outputPath;
        Instructions.Add($"  🔹 generate module,routing and menu.");
        Instructions.Add($"  🔹 generate pages.");

        var currentDir = new DirectoryInfo(dtoPath);
        var solutionFile = AssemblyHelper.GetSlnFile(currentDir, currentDir.Root)
            ?? throw new Exception("not found solution file");

        SolutionPath = solutionFile.DirectoryName!;
    }


    public ViewCommand(string entityPath, string dtoPath, string outputPath)
    {
        if (!File.Exists(entityPath))
        {
            throw new FileNotFoundException();
        }
        EntityFilePath = entityPath;
        DtoPath = dtoPath;
        OutputPath = outputPath;
        Instructions.Add($"  🔹 generate module, routing.");
        Instructions.Add($"  🔹 generate pages.");

        EntityName = Path.GetFileNameWithoutExtension(entityPath);
        var currentDir = new DirectoryInfo(dtoPath);
        var solutionFile = AssemblyHelper.GetSlnFile(currentDir, currentDir.Root)
            ?? throw new Exception("not found solution file");

        SolutionPath = solutionFile.DirectoryName!;
    }

    public void SetEntityPath(string entityPath)
    {
        EntityFilePath = entityPath;

        if (!File.Exists(entityPath))
        {
            throw new FileNotFoundException();
        }
        EntityName = Path.GetFileNameWithoutExtension(entityPath);
        Gen = new NgPageGenerate(EntityName, DtoPath, OutputPath);
    }

    public async Task RunAsync()
    {
        // 是否为模块
        var compilation = new CompilationHelper(DtoPath, "Entity");
        var content = File.ReadAllText(EntityFilePath);
        compilation.LoadContent(content);
        var attributes = compilation.GetClassAttribution("Module");
        var moduleName = string.Empty;
        if (attributes != null && attributes.Count != 0)
        {
            var argument = attributes.First().ArgumentList!.Arguments[0];
            moduleName = compilation.GetArgumentValue(argument);
        }
        if (!string.IsNullOrWhiteSpace(moduleName))
        {
            DtoPath = Path.Combine(SolutionPath, "src", "Modules", moduleName);
        }

        Gen = new NgPageGenerate(EntityName, DtoPath, OutputPath);

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
        var list = ModuleRouteMap.GroupBy(g => g.Key)
            .Select(g => new
            {
                module = g.Key,
                route = g.Select(g => g.Value).ToList()
            }).ToList();

        foreach (var item in list)
        {
            string moduleContent = NgPageGenerate.GetGroupModule(item.module.ToPascalCase(), item.route);
            string moduleRoutingContent = NgPageGenerate.GetGroupRoutingModule(item.module.ToPascalCase());

            string moduleFilename = item.module.ToHyphen() + ".module.ts";
            string routingFilename = item.module.ToHyphen() + "-routing.module.ts";
            string dir = Path.Combine(OutputPath, "src", "app", "pages", item.module.ToHyphen());
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
        string dir = Path.Combine(OutputPath, "src", "app", "components", "navigation");
        string fileName = "navigation.component.html";
        string content = await File.ReadAllTextAsync(Path.Combine(dir, fileName));


        var list = ModuleRouteMap.GroupBy(g => g.Key)
            .Select(g => new
            {
                module = g.Key,
                route = g.Select(g => g.Value).ToList(),
            }).ToList();

        string menusContent = "";
        foreach (var item in list)
        {
            menusContent += NgPageGenerate.GetNavigation(item.module.ToPascalCase(), item.route, RouteNameMap);
        }
        // 插入的起始点
        string startTmp = "<ng-template #automenu>";
        string endTmp = "</ng-template>";
        int startIndex = content.LastIndexOf(startTmp) + startTmp.Length;
        int endIndex = content.LastIndexOf(endTmp);
        StringBuilder sb = new();
        _ = sb.Append(content.AsSpan(0, startIndex));
        _ = sb.AppendLine();
        _ = sb.Append(menusContent);
        _ = sb.Append(content.AsSpan(endIndex));
        await GenerateFileAsync(dir, fileName, sb.ToString(), true);
    }


    private async Task GenerateModuleWithRoutingAsync()
    {
        string moduleName = ModuleName ?? EntityName.ToHyphen();
        string? routeName = Route?.ToPascalCase().ToHyphen();
        string dir = Path.Combine(OutputPath, moduleName, routeName ?? "");

        string module = Gen!.GetModule(Route?.ToPascalCase());
        string routing = Gen.GetRoutingModule(moduleName, Route?.ToPascalCase());
        string moduleFilename = routeName ?? moduleName + ".module.ts";
        string routingFilename = routeName ?? moduleName + "-routing.module.ts";
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
        string moduleName = ModuleName ?? EntityName.ToHyphen();
        string dir = Path.Combine(OutputPath, moduleName, Route?.ToPascalCase().ToHyphen() ?? "");

        NgComponentInfo addComponent = Gen!.BuildAddPage();
        NgComponentInfo editComponent = Gen.BuildEditPage();
        NgComponentInfo indexComponent = Gen.BuildIndexPage();
        NgComponentInfo detailComponent = Gen.BuildDetailPage();
        //NgComponentInfo layoutComponent = Gen.BuildLayout();
        //NgComponentInfo confirmDialogComponent = NgPageGenerate.BuildConfirmDialog();
        //string componentsModule = NgPageGenerate.GetComponentModule();

        await GenerateComponentAsync(dir, addComponent);
        await GenerateComponentAsync(dir, editComponent);
        await GenerateComponentAsync(dir, detailComponent);
        await GenerateComponentAsync(dir, indexComponent);

        //await GenerateComponentAsync(dir, layoutComponent);
        //await GenerateComponentAsync(dir, confirmDialogComponent);
        //await GenerateFileAsync(dir, "components.module.ts", componentsModule);
    }

    /// <summary>
    /// 生成组件文件
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="info"></param>
    /// <returns></returns>
    public async Task GenerateComponentAsync(string dir, NgComponentInfo info)
    {
        string path = Path.Combine(dir, info.Name);
        await GenerateFileAsync(path, info.Name + ".component.ts", info.TsContent!);
        await GenerateFileAsync(path, info.Name + ".component.scss", info.CssContent!);
        await GenerateFileAsync(path, info.Name + ".component.html", info.HtmlContent!);
    }
}
