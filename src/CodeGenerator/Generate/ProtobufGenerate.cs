using PropertyInfo = Core.Models.PropertyInfo;

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
        var content = $@"
service {EntityInfo.Name} {{
  rpc Filter (FilterDto) returns (PageDto);
  rpc Add (AddDto) returns ({EntityInfo.Name});
  rpc Update (UpdateDto) returns ({EntityInfo.Name});
  rpc Delete (IdDto) returns ({EntityInfo.Name});
  rpc Detail (IdDto) returns ({EntityInfo.Name});
}}
";
        return content;
    }

    /// <summary>
    /// C#属性转proto 字段
    /// </summary>
    /// <param name="property"></param>
    /// <param name="sort">默认排序值 </param>
    /// <returns></returns>
    internal static string ToProtobufField(PropertyInfo property, int sort)
    {
        if (ProtobufHelper.TypeMap.TryGetValue(property.Type, out var value))
        {
            return $@"    {value} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}";
        }
        return string.Empty;
    }

    /// <summary>
    /// 生成实体模型对应的message
    /// </summary>
    /// <returns></returns>
    public string GenerateMessages()
    {
        var sb = new StringBuilder();
        sb.Append("message ").Append(EntityInfo.Name);
        sb.Append(Environment.NewLine);
        sb.Append('{');
        sb.Append(Environment.NewLine);
        var fields = "";

        // 选择合适的属性
        for (int i = 0; i < EntityInfo.PropertyInfos.Count; i++)
        {
            var prop = EntityInfo.PropertyInfos[i];
            fields += ToProtobufField(prop, i + 1);
        }

        sb.Append('}');
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
