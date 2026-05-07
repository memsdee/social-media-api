using System.Security.Claims;
using System.Threading.RateLimiting;

namespace be.Api.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddApiRateLimit(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;

            options.AddPolicy("UserStrictPolicy", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserId(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 3,
                        QueueLimit = 0
                    }));

            options.AddPolicy("UserGeneralPolicy", httpContext =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    GetUserId(httpContext),
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 3,
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    private static string GetUserId(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? httpContext.Connection.RemoteIpAddress?.ToString()
               ?? "anonymous";
    }
}