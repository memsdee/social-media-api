using be.Application.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace be.Application.Extensions;

public static class MediatRExtensions
{
    public static IServiceCollection AddApplicationMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}