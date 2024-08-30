namespace CodeGenerator.Models;
public class ManagerViewModel
{
    public string? Namespace { get; set; }
    public string? EntityName { get; set; }
    public string? ShareNamespace { get; set; }
    public EntityInfo? EntityInfo { get; set; }
}
