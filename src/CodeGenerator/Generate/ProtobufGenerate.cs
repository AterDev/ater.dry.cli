namespace CodeGenerator.Generate;
internal class ProtobufGenerate : GenerateBase
{
    public EntityInfo EntityInfo { get; init; }
    public ProtobufGenerate(string entityPath, string dtoPath)
    {
        var entityHelper = new EntityParseHelper(entityPath);
        EntityInfo = entityHelper.GetEntity();
    }

    public string GenerateProtobuf()
    {
        return default;
    }
}
