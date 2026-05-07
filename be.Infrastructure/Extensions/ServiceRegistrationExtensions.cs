using be.Application.Interfaces.BackgroundJob;
using be.Application.Interfaces.External;
using be.Application.Interfaces.Generator;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Infrastructure.BackgroundService;
using be.Infrastructure.Services;
using be.Infrastructure.Services.Generator;
using Microsoft.Extensions.DependencyInjection;

namespace be.Infrastructure.Extensions;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenGenerator, JwtTokenGeneratorService>();
        services.AddScoped<IEmail, SmtpService>();
        services.AddScoped<IQuestion, ShuffleService>();
        services.AddScoped<IFormat, FormatService>();
        services.AddScoped<IScoreService, ScoreService>();
        services.AddScoped<IEncryption, EncryptService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasherService>();
        services.AddScoped<IRealtimeNotifier, RealtimeNotifierService>();
        services.AddScoped<IDefaultInfoGenerator, DefaultInfoGenerator>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<ITokenBgJob, TokenJob>();
        services.AddScoped<IPostBgJob, PostJob>();

        return services;
    }
}