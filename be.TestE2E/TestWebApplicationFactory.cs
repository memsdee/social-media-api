using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace be.TestE2E;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove the configuration that AddApiRateLimit added
            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();

            // Add our own permit-all configuration
            services.AddRateLimiter(options =>
            {
                options.AddPolicy("UserStrictPolicy", context => RateLimitPartition.GetNoLimiter("permit-all"));
                options.AddPolicy("UserGeneralPolicy", context => RateLimitPartition.GetNoLimiter("permit-all"));
            });
        });
    }
}