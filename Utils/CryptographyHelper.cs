using System.Security.Cryptography;
using System.Text;

namespace Khodgard.Utils;

public static class CryptographyHelper
{
    public static string HmacSha256(string key, string data)
    {
        string hash;
        ASCIIEncoding encoder = new();

        byte[] keyBytes = encoder.GetBytes(key);

        using HMACSHA256 hmac = new(keyBytes);
        byte[] dataBytes = hmac.ComputeHash(encoder.GetBytes(data));
        hash = ToHexString(dataBytes);

        return hash;
    }

    public static string ToHexString(byte[] data)
    {
        StringBuilder hex = new(data.Length * 2);
        foreach (var item in data)
        {
            hex.AppendFormat("{0:x2}", item);
        }

        return hex.ToString();
    }
}