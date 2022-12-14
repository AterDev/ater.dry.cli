using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
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
    public ContextBase Context { get; }

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
        Context = new ContextBase();
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
            NotifyFilter = NotifyFilters.LastWrite & NotifyFilters.Size,
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
            NotifyFilter = NotifyFilters.LastWrite & NotifyFilters.Size,
            Filter = "*.cs",
            EnableRaisingEvents = true
        };
        DtoWatcher.IncludeSubdirectories = true;

        DtoWatcher.Changed += OnDtoFileChanged;
        DtoWatcher.Error += OnError;
    }

    private async void OnFileCreatedAsync(object sender, FileSystemEventArgs e)
    {
        // 解析
        var entityparseHelper = new EntityParseHelper(e.FullPath);
        var baseType = entityparseHelper.GetParentClassName() ?? "";

        // 判断是否为实体
        if (baseType.Equals("EntityBase"))
        {
            // 添加入库
            var entityInfo = entityparseHelper.GetEntity();
            await Context.AddAsync(entityInfo);
            await Context.SaveChangesAsync();

            // 生成
            await CommandRunner.GenerateManagerAsync(e.FullPath, DtoPath, ApplicationPath);
        }
    }

    private static void OnFileDeleted(object sender, FileSystemEventArgs e) =>
        Console.WriteLine($"Deleted: {e.FullPath}");

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Renamed)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }
        else
        {
            Console.WriteLine(e.ChangeType.ToString());
        }
    }
    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine("on file changed:" + e.ChangeType + e.FullPath);
        if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            await CommandRunner.GenerateManagerAsync(e.FullPath, DtoPath, ApplicationPath);
        }
    }

    private void OnDtoFileChanged(object sender, FileSystemEventArgs e)
    {
        string value = $"Created: {e.FullPath}";
        Console.WriteLine("dto file change:" + e.FullPath);
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
