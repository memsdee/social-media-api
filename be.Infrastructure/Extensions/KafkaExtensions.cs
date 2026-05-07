using be.Application.Interfaces;
using be.Infrastructure.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace be.Infrastructure.Extensions;

public static class KafkaExtensions
{
    public static IServiceCollection AddInfrastructureKafka(this IServiceCollection services)
    {
        services.AddScoped<IEventBus, KafkaProducer>();

        return services;
    }
}