using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be.Infrastructure.Extensions;

public static class HangfireExtensions
{
    public static IServiceCollection AddInfrastructureHangfire(this IServiceCollection services,
        IConfiguration configuration)
    {
        var writeConnections = configuration.GetConnectionString("WriteConnections")
                               ?? throw new InvalidOperationException("WriteConnections configuration đang trống");

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(writeConnections))
        );

        services.AddHangfireServer();

        return services;
    }
}