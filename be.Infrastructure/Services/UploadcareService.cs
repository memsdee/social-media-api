using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using be.Application.Interfaces.External;
using be.Infrastructure.Common.Appsetting;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Services;

public class UploadcareService(IOptions<UploadcareSettings> setting, HttpClient httpClient) : IImage
{
    private readonly UploadcareSettings _settings = setting.Value;

    public Task<ImageUploadCredentials> SignatureAsync(CancellationToken cancellationToken)
    {
        var exp = DateTimeOffset.UtcNow.AddMinutes(_settings.ExpMinute).ToUnixTimeSeconds().ToString();

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(exp));

        return Task.FromResult(new ImageUploadCredentials
        {
            Exp = exp,
            Signature = Convert.ToHexString(signature).ToLowerInvariant(),
            PublicKey = _settings.PublicKey
        });
    }

    public async Task MoveImageAsync(List<Guid> fileId, CancellationToken cancellation)
    {
        var json = JsonSerializer.Serialize(fileId);
        using var payload = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PutAsync("storage/", payload, cancellation);

        if (!response.IsSuccessStatusCode)
            throw new Exception();
    }

    public async Task DeleteImageAsync(List<Guid> fileId, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(fileId);

        using var request = new HttpRequestMessage(HttpMethod.Delete, "storage/");

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await httpClient.SendAsync(
            request,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
            throw new Exception();
    }
}