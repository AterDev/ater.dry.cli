using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Command.Share;
public class WatcherManager
{
    public static Dictionary<string, FileWatcher> WatcherList { get; set; } = new();

    public static FileWatcher StartWatcher(string name, string entityPath, string dtoPath, string appPath)
    {
        if (WatcherList.ContainsKey(name))
        {
            return WatcherList[name];
        }
        else
        {
            var watcher = new FileWatcher(entityPath, dtoPath, appPath);
            Console.WriteLine("start new watcher:" + name);
            watcher.StartWatchers();
            WatcherList.Add(name, watcher);
            return watcher;
        }
    }

    public static void StopWatcher(string name)
    {
        if (WatcherList.TryGetValue(name, out var watcher))
        {
            watcher.StopWatchers();
            WatcherList.Remove(name);
            Console.WriteLine("stop watcher:" + name);
        }
    }
}
