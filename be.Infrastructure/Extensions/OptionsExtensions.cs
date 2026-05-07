using be.Infrastructure.Common.Appsetting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be.Infrastructure.Extensions;

public static class OptionsExtensions
{
    public static IServiceCollection AddInfrastructureOptions(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("JwtSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<EmailSettings>()
            .Bind(configuration.GetSection("EmailSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<DefaultInfoSettings>()
            .Bind(configuration.GetSection("DefaultInfoSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ScoreSettings>()
            .Bind(configuration.GetSection("ScoreSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<UploadcareSettings>()
            .Bind(configuration.GetSection("UploadcareSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<GcmSettings>()
            .Bind(configuration.GetSection("GCMSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OauthSettings>()
            .Bind(configuration.GetSection("OauthSettings"))
            .ValidateOnStart();

        services.AddOptions<KafkaSettings>()
            .Bind(configuration.GetSection("KafkaSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<DebeziumSettings>()
            .Bind(configuration.GetSection("DebeziumSettings"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<RedisCacheOptions>()
            .Bind(configuration.GetSection("RedisCacheOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}