using System.Security.Cryptography;
using System.Text;

namespace be.Domain.Helpers;

public class HashHelper
{
    public static string GetHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);

        var sb = new StringBuilder();
        foreach (var b in hashBytes) sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}