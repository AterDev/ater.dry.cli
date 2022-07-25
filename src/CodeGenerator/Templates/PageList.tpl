namespace ${Namespace}.Models;
public class PageList<T>
{
    public int Count { get; set; } = 0;
    public List<T> Data { get; set; } = new List<T>();
    public int PageIndex { get; set; } = 1;
}