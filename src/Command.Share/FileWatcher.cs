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
    /// TODO:.git\logs\HEAD 变动时间
    /// </summary>
    public DateTimeOffset GitChangeTime { get; private set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// 创建file watcher
    /// </summary>
    /// <param name="entityPath">实体目录</param>
    /// <param name="dtoPath">dto目录</param>
    /// <param name="appPath">应用层manager目录</param>
    public FileWatcher(string entityPath, string dtoPath, string appPath)
    {
        EntityPath = entityPath;
        DtoPath = dtoPath;
        ApplicationPath = appPath;
        GitChangeTime = GetGitLogLastWriteTime();
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
        FileInfo file = new(path);
        entityParseHelper = null;
        if (file.Length <= 10)
        {
            return false;
        }
        // 解析
        entityParseHelper = new EntityParseHelper(path);
        string baseType = entityParseHelper.GetParentClassName() ?? "";
        // 判断是否为实体
        return baseType.Equals("EntityBase");
    }

    private async void OnFileCreatedAsync(object sender, FileSystemEventArgs e)
    {
        // 判断是否为实体
        if (!IsGitChange()
            && e.FullPath.EndsWith(".cs"))
        {
            Console.WriteLine($"{e.Name} create!");
            if (IsEntityFile(e.FullPath, out _))
            {
                // 生成
                await CommandRunner.GenerateManagerAsync(e.FullPath, DtoPath, ApplicationPath);
            }
        }
    }

    private async void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        // 只有重命名为.cs文件后
        if (!IsGitChange()
            && e.ChangeType == WatcherChangeTypes.Renamed
            && e.FullPath.EndsWith(".cs"))
        {
            Console.WriteLine($"{e.Name} update!");
            if (IsEntityFile(e.FullPath, out _))
            {
                await CommandRunner.GenerateDtoAsync(e.FullPath, DtoPath, true);
            }
        }
    }

    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        FileInfo file = new(e.FullPath);
        if (!IsGitChange()
            && file.Length > 0
            && e.ChangeType == WatcherChangeTypes.Changed)
        {
            Console.WriteLine($"{e.Name} update!");
            if (IsEntityFile(e.FullPath, out _))
            {
                await CommandRunner.GenerateDtoAsync(e.FullPath, DtoPath, true);
            }
        }
    }

    private void OnDtoFileChanged(object sender, FileSystemEventArgs e)
    {
        //Console.WriteLine("dto file change:" + e.Name);
    }

    private static void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
    }

    private static void OnError(object sender, ErrorEventArgs e)
    {
        PrintException(e.GetException());
    }

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

    /// <summary>
    /// 获取git log 最后写入时间
    /// </summary>
    /// <returns></returns>
    private DateTimeOffset GetGitLogLastWriteTime()
    {
        DirectoryInfo? root = AssemblyHelper.GetGitRoot(new DirectoryInfo(EntityPath));
        if (root == null)
        {
            return DateTimeOffset.MinValue;
        }
        string filePath = Path.Combine(root.FullName, ".git", "logs", "HEAD");
        return new FileInfo(filePath).LastWriteTime;
    }

    private bool IsGitChange()
    {
        DateTimeOffset lastTime = GetGitLogLastWriteTime();
        if (lastTime == GitChangeTime)
        {
            return false;
        }
        else
        {
            Console.WriteLine("git change");
            GitChangeTime = lastTime;
            return true;
        }
    }
}
