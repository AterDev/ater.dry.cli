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

    public static string GenerateMessages()
    {
        var content = "";

        return content;
    }

    /// <summary>
    /// C#属性转proto字段
    /// </summary>
    /// <param name="property"></param>
    /// <param name="sort">默认排序值 </param>
    /// <returns></returns>
    internal static string ToProtobufField(PropertyInfo property, int sort)
    {
        // TODO:未处理字典情况
        // TODO:枚举类型
        if (property.IsEnum)
        {

        }

        if (ProtobufHelper.TypeMap.TryGetValue(property.Type, out var value))
        {
            if (property.IsList)
            {
                return $@"    repeated {value} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}";
            }
            return $@"    {value} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}";
        }
        else
        {
            if (property.IsList)
            {
                return $@"    repeated {property.Type} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}";
            }
            if (property.IsNavigation)
            {
                return $@"    {property.Type} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}";
            }
            return $@"    google.protobuf.Value {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}";
        }
    }

    /// <summary>
    /// 构建message
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    internal static string BuildMessage(string name, string fields)
    {
        var sb = new StringBuilder();
        sb.Append("message ").Append(name);
        sb.Append(Environment.NewLine);
        sb.Append('{');
        sb.Append(Environment.NewLine);
        sb.Append(fields);
        sb.Append('}');
        return sb.ToString();
    }

    /// <summary>
    /// 生成实体模型对应的message
    /// </summary>
    /// <returns></returns>
    public string GenerateEntityMessage()
    {
        var fields = "";
        // 选择合适的属性
        for (int i = 0; i < EntityInfo.PropertyInfos.Count; i++)
        {
            var prop = EntityInfo.PropertyInfos[i];
            fields += ToProtobufField(prop, i + 1);
        }
        return BuildMessage(EntityInfo.Name, fields);
    }

    public string GenerateFilterMessage()
    {
        var fields = @"
    int32 page_size = 1;
    int32 page_index = 2;
";
        string[] filterFields = new string[] { "Id", "CreatedTime", "UpdatedTime", "IsDeleted", "PageSize", "PageIndex" };
        var properties = EntityInfo.PropertyInfos?
            .Where(p => (p.IsRequired && !p.IsNavigation)
                || (!p.IsList
                    && !p.IsNavigation
                    && !filterFields.Contains(p.Name)
                    || p.IsEnum)
                )
            .Where(p => p.MaxLength is not (not null and >= 1000))
            .ToList();
        for (int i = 0; i < properties?.Count; i++)
        {
            var prop = properties[i];
            fields += ToProtobufField(prop, i + 3);
        }
        return BuildMessage("FilterDto", fields);
    }

    public string GenerateAddMessage()
    {
        var fields = "";
        var properties = EntityInfo.PropertyInfos?.Where(p => p.Name is not "Id"
                and not "CreatedTime"
                and not "UpdatedTime"
                and not "IsDeleted")
            .ToList();

        for (int i = 0; i < properties?.Count; i++)
        {
            var prop = properties[i];
            fields += ToProtobufField(prop, i + 1);
        }
        return BuildMessage("AddDto", fields);
    }

    public string GenerateUpdateMessage()
    {
        var fields = "";
        var properties = EntityInfo.PropertyInfos?.Where(p => p.Name is not "Id"
                and not "CreatedTime"
                and not "UpdatedTime"
                and not "IsDeleted")
            .ToList();

        for (int i = 0; i < properties?.Count; i++)
        {
            var prop = properties[i];
            prop.IsNullable = true;
            fields += ToProtobufField(prop, i + 1);
        }
        return BuildMessage("AddDto", fields);

    }

    public static string GenerateIdMessage()
    {
        var fields = "    string id = 1;";
        return BuildMessage("IdDto", fields);

    }

    public string GeneratePageMessage()
    {
        //        public int Count { get; set; } = 0;
        //public List<T> Data { get; set; } = new List<T>();
        //public int PageIndex { get; set; } = 1;
        var fields = $@"
    int32 count = 1;
    repeated {EntityInfo.Name} data = 2;
    int32 page_index = 3;";
        return BuildMessage("PageDto", fields);
    }
}
