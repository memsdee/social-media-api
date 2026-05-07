using be.Application.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace be.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddApplicationMediatR()
            .AddApplicationFluentValidation();

        return services;
    }
}