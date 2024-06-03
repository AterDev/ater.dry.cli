using System.Security.Cryptography;

namespace Definition.Infrastructure.Utils;

/// <summary>
/// hash加密
/// </summary>
public class CryptHelper
{
    public static string HashString(string str, HashType type)
    {
        var bytes = HashData(str, type);
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    private static byte[] HashData(string str, HashType type)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        return type switch
        {
            HashType.MD5 => MD5.HashData(bytes),
            HashType.SHA256 => SHA256.HashData(bytes),
            HashType.SHA512 => SHA512.HashData(bytes),
            _ => throw new NotSupportedException()
        };
    }
}
public enum HashType
{
    MD5,
    SHA256,
    SHA512,
}
