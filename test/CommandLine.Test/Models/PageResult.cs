namespace CommandLine.Test.Models
public class PageResult<T>
{
    public int Count { get; set; }
    public List<T>? Data { get; set; }
    public int PageIndex { get; set; }
}