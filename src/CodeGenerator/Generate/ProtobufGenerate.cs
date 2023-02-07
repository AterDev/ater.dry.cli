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
        var tpl = GetTplContent("protobuf.tpl");

        var services = GenerateServices();
        var messages = GenerateMessages();

        tpl = tpl.Replace(TplConst.PROTOBUF_SERVICES, services)
            .Replace(TplConst.PROTOBUF_MESSAGES, messages)
            .Replace(TplConst.PROTOBUF_NAMESPACE, EntityInfo.Name);

        return tpl;
    }

    public string GenerateServices()
    {
        var tpl = $@"
service {EntityInfo.Name} {{
  rpc Filter (FilterDto) returns (PageDto);
  rpc Add (AddDto) returns ({EntityInfo.Name});
  rpc Update (UpdateDto) returns ({EntityInfo.Name});
  rpc Delete (IdDto) returns ({EntityInfo.Name});
  rpc Detail (IdDto) returns ({EntityInfo.Name});
}}

";
        return "";
    }


    public string GenerateMessages()
    {
        return string.Empty;
    }

    public string GenerateFilterMessage()
    {
        return string.Empty;

    }

    public string GenerateAddMessage()
    {
        return string.Empty;

    }

    public string GenerateUpdateMessage()
    {
        return string.Empty;

    }
    public string GenerateIdMessage()
    {
        return string.Empty;

    }

    public string GeneratePageMessage()
    {
        return string.Empty;

    }
    public string GenerateEntityMessage()
    {
        return string.Empty;

    }
    public string GenerateListMessage()
    {
        return string.Empty;

    }
}
