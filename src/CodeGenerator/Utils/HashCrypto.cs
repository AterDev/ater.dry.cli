using System.Security.Cryptography;
using System.Text;

namespace CodeGenerator.Utils;

/// <summary>
/// hash加密
/// </summary>
public class HashCrypto
{
    public static string Create(string value, string salt)
    {
        var encrpty = new Rfc2898DeriveBytes(value, Encoding.UTF8.GetBytes(salt), 100, HashAlgorithmName.SHA512);
        var valueBytes = encrpty.GetBytes(32);
        return Convert.ToBase64String(valueBytes);
    }
    public static bool Validate(string value, string salt, string hash)
    {
        return Create(value, salt) == hash;
    }
    public static string BuildSalt()
    {
        var randomBytes = new byte[128 / 8];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
    /// <summary>
    /// 字符串md5值
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Md5Hash(string str)
    {
        using var md5 = MD5.Create();
        var data = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
        var sBuilder = new StringBuilder();
        for (var i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
    /// <summary>
    /// 某文件的md5值
    /// </summary>
    /// <param name="stream">file stream</param>
    /// <returns></returns>
    public static string Md5FileHash(Stream stream)
    {
        using var md5 = MD5.Create();
        var data = md5.ComputeHash(stream);
        var sBuilder = new StringBuilder();
        for (var i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
    /// <summary>
    /// 生成随机数
    /// </summary>
    /// <param name="length"></param>
    /// <param name="useNum"></param>
    /// <param name="useLow"></param>
    /// <param name="useUpp"></param>
    /// <param name="useSpe"></param>
    /// <param name="custom"></param>
    /// <returns></returns>
    public static string GetRnd(int length = 4, bool useNum = true, bool useLow = false, bool useUpp = true, bool useSpe = false, string custom = "")
    {
        var b = new byte[4];
        new RNGCryptoServiceProvider().GetBytes(b);
        var r = new Random(BitConverter.ToInt32(b, 0));
        string s = null, str = custom;
        if (useNum) { str += "0123456789"; }
        if (useLow) { str += "abcdefghijklmnopqrstuvwxyz"; }
        if (useUpp) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
        if (useSpe) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
        for (var i = 0; i < length; i++)
        {
            s += str.Substring(r.Next(0, str.Length - 1), 1);
        }
        return s;
    }
}
