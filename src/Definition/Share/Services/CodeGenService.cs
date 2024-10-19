using System.ComponentModel;
using System.Diagnostics;
using CodeGenerator.Generate;
using CodeGenerator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Share.Services;
/// <summary>
/// 代码生成服务
/// </summary>
public class CodeGenService(ILogger<CodeGenService> logger)
{
    private readonly ILogger<CodeGenService> _logger = logger;

    /// <summary>
    /// 生成Dto
    /// </summary>
    /// <param name="entityInfo">实体信息</param>
    /// <param name="outputPath">输出项目目录</param>
    /// <param name="isCover">是否覆盖</param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateDtos(EntityInfo entityInfo, string outputPath, bool isCover = false)
    {
        _logger.LogInformation("🚀 Generating Dtos...");
        // 生成Dto
        var dtoGen = new DtoCodeGenerate(entityInfo);
        var dirName = entityInfo.Name + "Dtos";
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, dtoGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };
        return
        [
            globalFile,
            new GenFileInfo($"{entityInfo.Name}{ConstVal.AddDto}.cs", dtoGen.GetAddDto())
            {
                IsCover = isCover,
                FullName = Path.Combine(outputPath, ConstVal.ModelsDir, dirName, $"{entityInfo.Name}{ConstVal.AddDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo( $"{entityInfo.Name}{ConstVal.UpdateDto}.cs", dtoGen.GetUpdateDto())
            {
                IsCover = isCover,
                FullName = Path.Combine(outputPath, ConstVal.ModelsDir, dirName, $"{entityInfo.Name}{ConstVal.UpdateDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo( $"{entityInfo.Name}{ConstVal.FilterDto}.cs", dtoGen.GetFilterDto())
            {
                IsCover = isCover,
                FullName = Path.Combine(outputPath, ConstVal.ModelsDir, dirName, $"{entityInfo.Name}{ConstVal.FilterDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo($"{entityInfo.Name}{ConstVal.ItemDto}.cs", dtoGen.GetItemDto())
            {
                IsCover = isCover,
                FullName = Path.Combine(outputPath, ConstVal.ModelsDir, dirName, $"{entityInfo.Name}{ConstVal.ItemDto}.cs"),
                ModuleName = entityInfo.ModuleName
            },
            new GenFileInfo($"{entityInfo.Name}{ConstVal.DetailDto}.cs", dtoGen.GetDetailDto())
            {
                IsCover = isCover,
                FullName = Path.Combine(outputPath, ConstVal.ModelsDir, dirName, $"{entityInfo.Name}{ConstVal.DetailDto}.cs"),
                ModuleName = entityInfo.ModuleName
            }
        ];
    }

    /// <summary>
    /// 生成manager的文件
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="outputPath"></param>
    /// <param name="tplContent">模板内容</param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateManager(EntityInfo entityInfo, string outputPath, string tplContent, bool isCover = false)
    {
        var managerGen = new ManagerGenerate(entityInfo);
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, managerGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };

        var content = managerGen.GetManagerContent(tplContent, entityInfo.GetManagerNamespace());
        var managerFile = new GenFileInfo($"{entityInfo.Name}{ConstVal.Manager}.cs", content)
        {
            IsCover = isCover,
            FullName = Path.Combine(outputPath, ConstVal.ManagersDir, $"{entityInfo.Name}{ConstVal.Manager}.cs"),
            ModuleName = entityInfo.ModuleName
        };

        var managerDIFile = GetManagerService(entityInfo, outputPath);
        return [globalFile, managerFile, managerDIFile];
    }

    /// <summary>
    /// Manager服务注入内容
    /// </summary>
    /// <returns></returns>
    public GenFileInfo GetManagerService(EntityInfo entityInfo, string outputPath)
    {
        var managerPath = Path.Combine(outputPath, ConstVal.ManagersDir);
        string content = ManagerGenerate.GetManagerServiceContent(managerPath, entityInfo.ModuleName);
        string name = entityInfo.ModuleName.IsEmpty()
            ? ConstVal.ManagerServiceExtensionsFile
            : ConstVal.ServiceExtensionsFile;

        return new GenFileInfo(name, content)
        {
            IsCover = true,
            FullName = Path.Combine(outputPath, name),
            ModuleName = entityInfo.ModuleName
        };
    }

    /// <summary>
    /// RestAPI生成
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="outputPath"></param>
    /// <param name="tplContent"></param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateController(EntityInfo entityInfo, string outputPath, string tplContent, bool isCover = false)
    {
        var apiGen = new RestApiGenerate(entityInfo);
        // GlobalUsing

        var globalFilePath = Path.Combine(outputPath, ConstVal.GlobalUsingsFile);
        var globalLines = File.Exists(globalFilePath)
            ? File.ReadLines(globalFilePath)
            : [];
        var globalList = apiGen.GetGlobalUsings();
        // add globalList  item if globalLines not exist 
        globalList.ForEach(g =>
        {
            if (!globalLines.Contains(g))
            {
                globalLines.Append(g);
            }
        });

        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, string.Join(Environment.NewLine, globalLines))
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };
        var content = apiGen.GetRestApiContent(tplContent);
        var controllerFile = new GenFileInfo($"{entityInfo.Name}{ConstVal.Controller}.cs", content)
        {
            IsCover = isCover,
            FullName = Path.Combine(outputPath, ConstVal.ControllersDir, $"{entityInfo.Name}{ConstVal.Controller}.cs"),
            ModuleName = entityInfo.ModuleName
        };
        return [globalFile, controllerFile];
    }

    public async Task<List<GenFileInfo>> GenerateWebRequestAsync(string url = "", string outputPath = "", RequestLibType type = RequestLibType.NgHttp)
    {
        _logger.LogInformation("🚀 Generating ts models and {type} request services...", type);
        var files = new List<GenFileInfo>();

        // 1 parse openApi json from url
        string openApiContent = "";
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            using HttpClient http = new(handler);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            openApiContent = await http.GetStringAsync(url);
            stopwatch.Stop();
            _logger.LogInformation("⬇️ Get OpenAPI from {url} {seconds} seconds", url, stopwatch.Elapsed.TotalSeconds);
        }
        else
        {
            openApiContent = File.ReadAllText(url);
        }
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");

        var apiDocument = new OpenApiStringReader().Read(openApiContent, out _);
        var docName = url.Contains("http")
            ? url.Split('/').Reverse().Skip(1).First()
            : string.Empty;

        // base service
        string content = RequestGenerate.GetBaseService(type);
        string dir = Path.Combine(outputPath, "services", docName);
        files.Add(new GenFileInfo("base.service.ts", content)
        {
            FullName = Path.Combine(dir, "base.service.ts"),
            IsCover = false
        });

        // 枚举pipe
        if (type == RequestLibType.NgHttp)
        {
            IDictionary<string, OpenApiSchema> schemas = apiDocument!.Components.Schemas;
            string pipeContent = RequestGenerate.GetEnumPipeContent(schemas);
            dir = Path.Combine(outputPath, "pipe", docName);

            files.Add(new GenFileInfo("enum-text.pipe.ts", pipeContent)
            {
                FullName = Path.Combine(dir, "enum-text.pipe.ts"),
                IsCover = true
            });
        }
        // request services
        var ngGen = new RequestGenerate(apiDocument!)
        {
            LibType = type
        };
        // 获取对应的ts模型类，生成文件
        var tsModels = ngGen.GetTSInterfaces();
        tsModels.ForEach(m =>
        {
            dir = Path.Combine(outputPath, "services", docName, m.FullName, "models");
            m.FullName = Path.Combine(dir, m.Name);
            m.IsCover = true;
        });
        files.AddRange(tsModels);
        // 获取请求服务并生成文件
        var services = ngGen.GetServices(apiDocument!.Tags);
        services.ForEach(s =>
        {
            dir = Path.Combine(outputPath, "services", docName, s.FullName);
            s.FullName = Path.Combine(dir, s.Name);
            s.IsCover = true;
        });
        return files;
    }

    /// <summary>
    /// 生成文件
    /// </summary>
    /// <param name="files"></param>
    public void GenerateFiles(List<GenFileInfo>? files)
    {
        if (files == null || files.Count == 0)
        {
            return;
        }
        foreach (var file in files)
        {
            if (file.IsCover || !File.Exists(file.FullName))
            {
                var dir = Path.GetDirectoryName(file.FullName);
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir!);
                }
                File.WriteAllText(file.FullName, file.Content, Encoding.UTF8);
                _logger.LogInformation($"🆕 生成文件：{file.FullName}");
            }
        }
    }
}

public enum DtoType
{
    [Description("Add")]
    Add,
    [Description("Update")]
    Update,
    [Description("Filter")]
    Filter,
    [Description("Item")]
    Item,
    [Description("Detail")]
    Detail
}