using be.Infrastructure.BackgroundService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be.Infrastructure.Extensions;

public static class DebeziumExtensions
{
    public static IServiceCollection AddInfrastructureDebezium(
        this IServiceCollection services, IConfiguration configuration)
    {
        var url = configuration["DebeziumSettings:KafkaConnectUrl"]!;

        services.AddHttpClient("debezium", client => { client.BaseAddress = new Uri(url); });

        services.AddHostedService<DebeziumConnectorInitializer>();
        return services;
    }
}