using Datastore;

namespace Command.Share;
/// <summary>
/// 文件监控变更
/// </summary>
public class FileWatcher
{
    public FileSystemWatcher? EntityWatcher { get; private set; }
    public FileSystemWatcher? DtoWatcher { get; private set; }
    public string EntityPath { get; }
    public string DtoPath { get; }
    public string ApplicationPath { get; }
    public required Guid ProjectId { get; init; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entityPath">实体目录</param>
    /// <param name="dtoPath">dto目录</param>
    /// <param name="appPath">应用层manager目录</param>
    public FileWatcher(string entityPath, string dtoPath, string appPath)
    {
        EntityPath = entityPath;
        DtoPath = dtoPath;
        ApplicationPath = appPath;
    }

    public void StartWatchers()
    {
        WatchEntity();
        WatchDto();
    }

    public void StopWatchers()
    {
        if (EntityWatcher != null)
        {
            EntityWatcher.Created -= OnFileChanged;
            EntityWatcher.Changed -= OnFileChanged;
            EntityWatcher.Deleted -= OnFileChanged;
            EntityWatcher.Renamed -= OnFileRenamed;
            EntityWatcher.Dispose();
            EntityWatcher = null;
        }

        if (DtoWatcher != null)
        {
            DtoWatcher.Changed -= OnDtoFileChanged;
            DtoWatcher.Dispose();
            DtoWatcher = null;
        }
    }

    public void WatchEntity()
    {
        EntityWatcher = new FileSystemWatcher(Path.Combine(EntityPath, "Entities"))
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
            Filter = "*.cs",
            EnableRaisingEvents = true
        };
        EntityWatcher.IncludeSubdirectories = true;

        EntityWatcher.Created += OnFileCreatedAsync;
        EntityWatcher.Changed += OnFileChanged;
        EntityWatcher.Deleted += OnFileDeleted;
        EntityWatcher.Renamed += OnFileRenamed;
        EntityWatcher.Error += OnError;
    }
    public void WatchDto()
    {
        DtoWatcher = new FileSystemWatcher(Path.Combine(DtoPath, "Models"))
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
            Filter = "*.cs",
            EnableRaisingEvents = true
        };
        DtoWatcher.IncludeSubdirectories = true;

        DtoWatcher.Changed += OnDtoFileChanged;
        DtoWatcher.Error += OnError;
    }

    /// <summary>
    ///  判断文件是否为实体
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private bool IsEntityFile(string path, out EntityParseHelper? entityParseHelper)
    {
        var file = new FileInfo(path);
        entityParseHelper = null;
        if (file.Length <= 10)
        {
            return false;
        }
        // 解析
        entityParseHelper = new EntityParseHelper(path);
        var baseType = entityParseHelper.GetParentClassName() ?? "";
        // 判断是否为实体
        return baseType.Equals("EntityBase");
    }

    private async void OnFileCreatedAsync(object sender, FileSystemEventArgs e)
    {
        // 判断是否为实体
        if (IsEntityFile(e.FullPath, out var entityParseHelper))
        {
            // 添加入库
            var entityInfo = entityParseHelper!.GetEntity();
            using var Context = new DbContext();

            Context.EntityInfos.EnsureIndex(e => e.Name);
            Context.EntityInfos.Insert(entityInfo);

            // 生成
            await CommandRunner.GenerateManagerAsync(e.FullPath, DtoPath, ApplicationPath);
        }
    }

    private static void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
    }

    private async void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        // 只有重命名为.cs文件后
        if (e.ChangeType == WatcherChangeTypes.Renamed && e.FullPath.EndsWith(".cs"))
        {
            Console.WriteLine($"{e.Name} update!");

            if (IsEntityFile(e.FullPath, out var entityParseHelper))
            {
                await CommandRunner.GenerateDtoAsync(e.FullPath, DtoPath, true);
            }
        }
    }
    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var file = new FileInfo(e.FullPath);
        if (file.Length > 0 && e.ChangeType == WatcherChangeTypes.Changed)
        {
            Console.WriteLine($"{e.Name} update!");

            if (IsEntityFile(e.FullPath, out var entityParseHelper))
            {
                await CommandRunner.GenerateDtoAsync(e.FullPath, DtoPath, true);
            }
        }
    }

    private void OnDtoFileChanged(object sender, FileSystemEventArgs e)
    {
        //Console.WriteLine("dto file change:" + e.Name);
    }

    private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

    private static void PrintException(Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine("Stacktrace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
            PrintException(ex.InnerException);
        }
    }
}
