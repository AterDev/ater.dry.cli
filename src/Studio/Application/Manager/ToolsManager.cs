using System.ComponentModel;
using System.Net;
using System.Text;
using Definition.Infrastructure.Utils;

namespace Application.Manager;

/// <summary>
/// 工具类
/// </summary>
public class ToolsManager
{

    public ToolsManager()
    {

    }


    public List<string>? ConvertToClass(string json)
    {
        if (CSharpCovertHelper.CheckJson(json))
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            var helper = new CSharpCovertHelper();
            helper.GenerateClass(jsonElement);
            return helper.ClassCodes;
        }
        return null;
    }


    public Dictionary<string, string> ConvertString(string content, StringConvertType type)
    {
        var res = type switch
        {
            StringConvertType.NamePolicy =>
                    new Dictionary<string, string>
                    {
                        { "PascalCase", content.ToPascalCase() },
                        { "HyphenCase", content.ToHyphen() },
                        { "CamelCase", content.ToCamelCase() },
                    },

            StringConvertType.Guid => new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() }
            },
            StringConvertType.Encode => new Dictionary<string, string>
            {
                { "Base64", Convert.ToBase64String(Encoding.UTF8.GetBytes(content)) },
                { "UrlEncode", WebUtility.UrlEncode(content) },
                { "HtmlEncode", WebUtility.HtmlEncode(content) },
            },
            StringConvertType.Decode => new Dictionary<string, string>
            {
                { "Base64",  content.FromBase64String()??""},
                { "UrlDecode", WebUtility.UrlDecode(content) },
                { "HtmlDecode", WebUtility.HtmlDecode(content) },
            },
            StringConvertType.EnCrypt => new Dictionary<string, string>
            {
                { "MD5", HashCrypto.Md5Hash(content) },
                { "SHA256", CryptHelper.HashString(content, HashType.SHA256) },
                { "SHA512", CryptHelper.HashString(content, HashType.SHA512) },
            },
            _ => throw new NotImplementedException(),
        };

        return res;
    }
}


public enum StringConvertType
{
    /// <summary>
    /// Guid
    /// </summary>
    [Description("Guid")]
    Guid,
    /// <summary>
    /// 命名转换
    /// </summary>
    [Description("命名转换")]
    NamePolicy,
    /// <summary>
    /// 编码
    /// </summary>
    [Description("编码")]
    Encode,
    /// <summary>
    /// 解码
    /// </summary>
    [Description("解码")]
    Decode,
    /// <summary>
    /// 加密
    /// </summary>
    [Description("加密")]
    EnCrypt,
}