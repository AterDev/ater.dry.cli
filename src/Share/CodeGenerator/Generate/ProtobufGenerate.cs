using PropertyInfo = Definition.Entity.PropertyInfo;

namespace CodeGenerator.Generate;
public class ProtobufGenerate : GenerateBase
{
    public EntityInfo EntityInfo { get; init; }
    public EntityParseHelper EntityHelper { get; init; }
    public List<string> EnumList { get; set; } = [];

    public ProtobufGenerate(string entityPath)
    {
        EntityHelper = new EntityParseHelper(entityPath);
        EntityInfo = EntityHelper.GetEntity();
    }

    public string GenerateProtobuf()
    {
        string tpl = GetTplContent("Protobuf.tpl");

        string services = GenerateServices();
        string messages = GenerateMessages();

        tpl = tpl.Replace(TplConst.PROTOBUF_SERVICES, services)
            .Replace(TplConst.PROTOBUF_MESSAGES, messages)
            .Replace(TplConst.PROTOBUF_NAMESPACE, EntityInfo.Name);

        return tpl;
    }

    public string GenerateServices()
    {
        string content = $@"
service {EntityInfo.Name} {{
  rpc Filter (FilterRequest) returns (PageReply);
  rpc Add (AddRequest) returns ({EntityInfo.Name}Reply);
  rpc Update (UpdateRequest) returns ({EntityInfo.Name}Reply);
  rpc Delete (IdRequest) returns ({EntityInfo.Name}Reply);
  rpc Detail (IdRequest) returns ({EntityInfo.Name}Reply);
}}
";
        return content;
    }

    public string GenerateMessages()
    {
        string content = "";
        content += GenerateEntityMessage();
        content += GenerateFilterMessage();
        content += GenerateAddMessage();
        content += GenerateUpdateMessage();
        content += GenerateIdMessage();
        content += GeneratePageMessage();
        content += GenerateEnumMessage();
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
        return ProtobufHelper.TypeMap.TryGetValue(property.Type, out var value)
            ? property.IsList
                ? $@"    repeated {value} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}"
                : $@"    {value} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}"
            : property.IsList
                ? $@"    repeated {EntityParseHelper.GetTypeFromList(property.Type)} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}"
                : property.IsEnum || property.IsNavigation
                ? $@"    {property.Type} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}"
                : $@"    google.protobuf.Value {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}";
    }

    /// <summary>
    /// 构建message
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    internal static string BuildMessage(string name, string fields)
    {
        StringBuilder sb = new();
        sb.Append("message ").Append(name);
        sb.Append(Environment.NewLine);
        sb.Append('{');
        sb.Append(Environment.NewLine);
        sb.Append(fields);
        sb.Append('}');
        sb.Append(Environment.NewLine);
        sb.Append(Environment.NewLine);
        return sb.ToString();
    }

    /// <summary>
    /// 构建 enum message
    /// </summary>
    /// <param name="name"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    internal static string BuildEnumMessage(string name, List<IFieldSymbol?> fields)
    {
        StringBuilder sb = new();
        sb.Append("enum ").Append(name);
        sb.Append(Environment.NewLine);
        sb.Append('{');
        sb.Append(Environment.NewLine);
        string content = "";
        fields.ForEach(f =>
        {
            if (f != null)
                content += $"    {f.Name} = {f.ConstantValue};{Environment.NewLine}";

        });
        sb.Append(content);
        sb.Append('}');
        sb.Append(Environment.NewLine);
        return sb.ToString();
    }

    /// <summary>
    /// 生成枚举相关的message
    /// </summary>
    /// <returns></returns>
    public string GenerateEnumMessage()
    {
        string content = "";
        for (int i = 0; i < EntityInfo.PropertyInfos.Count; i++)
        {
            Entity.PropertyInfo prop = EntityInfo.PropertyInfos[i];
            // 枚举类型
            if (prop.IsEnum)
            {
                if (!EnumList.Any(e => e == prop.Type))
                {
                    List<IFieldSymbol?>? members = EntityHelper.GetEnumMembers(prop.Type);
                    if (members != null)
                    {
                        EnumList.Add(prop.Type);
                        content += BuildEnumMessage(prop.Type, members);
                    }
                }
            }
        }
        return content;
    }

    /// <summary>
    /// 生成实体模型对应的message
    /// </summary>
    /// <returns></returns>
    public string GenerateEntityMessage()
    {
        string fields = "";
        // 选择合适的属性
        for (int i = 0; i < EntityInfo.PropertyInfos.Count; i++)
        {
            Entity.PropertyInfo prop = EntityInfo.PropertyInfos[i];
            fields += ToProtobufField(prop, i + 1);
        }
        return BuildMessage(EntityInfo.Name + "Reply", fields);
    }

    public string GenerateFilterMessage()
    {
        string fields =
@"    int32 page_size = 1;
    int32 page_index = 2;
";
        string[] filterFields = ["Id", "CreatedTime", "UpdatedTime", "IsDeleted", "PageSize", "PageIndex"];
        List<Entity.PropertyInfo>? properties = EntityInfo.PropertyInfos?
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
            Entity.PropertyInfo prop = properties[i];
            fields += ToProtobufField(prop, i + 3);
        }
        return BuildMessage("FilterRequest", fields);
    }

    public string GenerateAddMessage()
    {
        string fields = "";
        List<Entity.PropertyInfo>? properties = EntityInfo.PropertyInfos?.Where(p => p.Name is not "Id"
                and not "CreatedTime"
                and not "UpdatedTime"
                and not "IsDeleted")
            .ToList();

        for (int i = 0; i < properties?.Count; i++)
        {
            Entity.PropertyInfo prop = properties[i];
            fields += ToProtobufField(prop, i + 1);
        }
        return BuildMessage("AddRequest", fields);
    }

    public string GenerateUpdateMessage()
    {
        string fields = "";
        List<Entity.PropertyInfo>? properties = EntityInfo.PropertyInfos?.Where(p => p.Name is not "Id"
                and not "CreatedTime"
                and not "UpdatedTime"
                and not "IsDeleted")
            .ToList();

        for (int i = 0; i < properties?.Count; i++)
        {
            Entity.PropertyInfo prop = properties[i];
            prop.IsNullable = true;
            fields += ToProtobufField(prop, i + 1);
        }
        return BuildMessage("UpdateRequest", fields);

    }

    public static string GenerateIdMessage()
    {
        string fields = "    string id = 1;" + Environment.NewLine;
        return BuildMessage("IdRequest", fields);

    }
    public string GeneratePageMessage()
    {
        //        public int Count { get; set; } = 0;
        //public List<T> Data { get; set; } = new List<T>();
        //public int PageIndex { get; set; } = 1;
        string fields = $@"
    int32 count = 1;
    repeated {EntityInfo.Name}Reply data = 2;
    int32 page_index = 3;
";
        return BuildMessage("PageReply", fields);
    }
}
