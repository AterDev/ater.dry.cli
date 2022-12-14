using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public string AppPath { get; }

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
        AppPath = appPath;
    }

    public void StopWathers()
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
        EntityWatcher = new FileSystemWatcher(EntityPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                            | NotifyFilters.LastWrite
                            | NotifyFilters.Security
                            | NotifyFilters.Size,
            Filter = "*.cs"
        };

        EntityWatcher.IncludeSubdirectories = true;
        EntityWatcher.Created += OnFileCreated;
        EntityWatcher.Changed += OnFileChanged;
        EntityWatcher.Deleted += OnFileChanged;
        EntityWatcher.Renamed += OnFileRenamed;
    }
    public void WatchDto(string path)
    {
        DtoWatcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName
                            | NotifyFilters.LastWrite
                            | NotifyFilters.Security
                            | NotifyFilters.Size,
            Filter = "*.cs"
        };
        DtoWatcher.IncludeSubdirectories = true;
        DtoWatcher.Changed += OnDtoFileChanged;
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        string value = $"Created: {e.FullPath}";

    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        throw new NotImplementedException();
    }
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
        {
            return;
        }
    }

    private void OnDtoFileChanged(object sender, FileSystemEventArgs e)
    {
        string value = $"Created: {e.FullPath}";
    }
}
