using System.Security.Cryptography;
using System.Text;
using be.Application.Interfaces.Security;
using be.Infrastructure.Common.Appsetting;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Services;

public class EncryptService(IOptions<GcmSettings> gcmSetting) : IEncryption
{
    private readonly byte[] _gcmSecret = Convert.FromBase64String(gcmSetting.Value.Key);

    public string Encrypt(string text)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(text);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_gcmSecret, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        return Convert.ToBase64String(nonce.Concat(tag).Concat(cipher).ToArray());
    }

    public string Decrypt(string text)
    {
        var data = Convert.FromBase64String(text);
        var nonce = data[..12];
        var tag = data[12..28];
        var cipher = data[28..];

        var plaintext = new byte[cipher.Length];

        using var aes = new AesGcm(_gcmSecret, tag.Length);
        aes.Decrypt(nonce, cipher, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }
}