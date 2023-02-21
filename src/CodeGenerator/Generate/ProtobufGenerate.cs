using PropertyInfo = Core.Models.PropertyInfo;

namespace CodeGenerator.Generate;
public class ProtobufGenerate : GenerateBase
{
    public EntityInfo EntityInfo { get; init; }
    public EntityParseHelper EntityHelper { get; init; }
    public List<string> EnumList { get; set; } = new List<string>();

    public ProtobufGenerate(string entityPath)
    {
        EntityHelper = new EntityParseHelper(entityPath);
        EntityInfo = EntityHelper.GetEntity();
    }

    public string GenerateProtobuf()
    {
        var tpl = GetTplContent("Protobuf.tpl");

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
        var content = "";
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
                return $@"    repeated {EntityParseHelper.GetTypeFromList(property.Type)} {property.Name.ToHyphen('_')} = {sort};{Environment.NewLine}";
            }
            if (property.IsEnum || property.IsNavigation)
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
        var sb = new StringBuilder();
        sb.Append("enum ").Append(name);
        sb.Append(Environment.NewLine);
        sb.Append('{');
        sb.Append(Environment.NewLine);
        var content = "";
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
        var content = "";
        for (int i = 0; i < EntityInfo.PropertyInfos.Count; i++)
        {
            var prop = EntityInfo.PropertyInfos[i];
            // 枚举类型
            if (prop.IsEnum)
            {
                if (!EnumList.Any(e => e == prop.Type))
                {
                    var members = EntityHelper.GetEnumMembers(prop.Type);
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
        var fields = "";
        // 选择合适的属性
        for (int i = 0; i < EntityInfo.PropertyInfos.Count; i++)
        {
            var prop = EntityInfo.PropertyInfos[i];
            fields += ToProtobufField(prop, i + 1);
        }
        return BuildMessage(EntityInfo.Name + "Reply", fields);
    }

    public string GenerateFilterMessage()
    {
        var fields =
@"    int32 page_size = 1;
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
        return BuildMessage("FilterRequest", fields);
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
        return BuildMessage("AddRequest", fields);
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
        return BuildMessage("UpdateRequest", fields);

    }

    public static string GenerateIdMessage()
    {
        var fields = "    string id = 1;" + Environment.NewLine;
        return BuildMessage("IdRequest", fields);

    }
    public string GeneratePageMessage()
    {
        //        public int Count { get; set; } = 0;
        //public List<T> Data { get; set; } = new List<T>();
        //public int PageIndex { get; set; } = 1;
        var fields = $@"
    int32 count = 1;
    repeated {EntityInfo.Name}Reply data = 2;
    int32 page_index = 3;
";
        return BuildMessage("PageReply", fields);
    }
}
