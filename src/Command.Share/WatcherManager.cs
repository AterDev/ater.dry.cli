namespace Command.Share;
public class WatcherManager
{
    public static Dictionary<Guid, FileWatcher> WatcherList { get; set; } = new();

    public static FileWatcher StartWatcher(Guid name, string entityPath, string dtoPath, string appPath)
    {
        if (WatcherList.TryGetValue(name, out FileWatcher? value))
        {
            return value;
        }
        else
        {
            var watcher = new FileWatcher(entityPath, dtoPath, appPath) { ProjectId = name };
            Console.WriteLine("start new watcher:" + name);
            watcher.StartWatchers();
            WatcherList.Add(name, watcher);
            return watcher;
        }
    }

    public static void StopWatcher(Guid name)
    {
        if (WatcherList.TryGetValue(name, out var watcher))
        {
            watcher.StopWatchers();
            WatcherList.Remove(name);
            Console.WriteLine("stop watcher:" + name);
        }
    }
}
