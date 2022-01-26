namespace ${Namespace}.Models;
public class BatchUpdate<T>
{
    public List<Guid> Ids { get; set; } = null!;
    public T UpdateDto { get; set; } = default!;
}
