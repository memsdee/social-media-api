using System.Text.Encodings.Web;
using be.Api.Services;
using be.Application.Interfaces.Security;

namespace be.Api.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var myCors = "_myAllowSpecificOrigins";

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, CurrentCurrentUserContext>();

        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

        services.AddEndpointsApiExplorer();
        services.AddSignalR();
        services.AddExceptionHandler<GlobalExceptionMidleware>();
        services.AddProblemDetails();

        services
            .AddApiCors(configuration, myCors)
            .AddApiSwagger()
            .AddApiJwt(configuration)
            .AddApiRateLimit();

        return services;
    }
}