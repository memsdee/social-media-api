using be.Application.Interfaces.External;
using be.Infrastructure.Common.Appsetting;
using be.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Extensions;

public static class HttpClientExtensions
{
    public static IServiceCollection AddInfrastructureHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IImage, UploadcareService>((provider, client) =>
        {
            var uploadcareSetting = provider.GetRequiredService<IOptions<UploadcareSettings>>().Value;
            client.BaseAddress = new Uri(uploadcareSetting.UrlApi);
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.uploadcare-v0.7+json");
            client.DefaultRequestHeaders.Add("Authorization",
                $"Uploadcare.Simple {uploadcareSetting.PublicKey}:{uploadcareSetting.SecretKey}");
        });

        services.AddHttpClient<IGoogle, GoogleService>(client =>
        {
            client.BaseAddress = new Uri("https://oauth2.googleapis.com/");
        });

        return services;
    }
}