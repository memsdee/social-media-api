using be.Infrastructure.Common.Appsetting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace be.Infrastructure.Extensions;

public static class RedisExtensions
{
    public static IServiceCollection AddInfrastructureRedis(this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection("RedisCacheOptions").Get<RedisCacheOptions>()
                           ?? throw new InvalidOperationException("RedisCacheOptions configuration đang trống");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisOptions.Configuration;
            options.InstanceName = redisOptions.InstanceName;
        });

        return services;
    }
}