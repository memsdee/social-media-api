using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace be.Application.Extensions;

public static class FluentValidationExtensions
{
    public static IServiceCollection AddApplicationFluentValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}