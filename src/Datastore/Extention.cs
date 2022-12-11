namespace Datastore;

public static class Extention
{
    public static string ToFullPath(this string path, string route, string dir)
    {
        return Path.Combine(dir, route, path);
    }
}
