using System.Security.Claims;
using System.Text;
using be.Infrastructure.Common.Appsetting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace be.Api.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddApiJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSetting = configuration.GetSection("JwtSettings").Get<JwtSettings>()
                         ?? throw new InvalidOperationException("JwtSettings configuration đang trống");

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.Key)),
                    ValidAudience = jwtSetting.Audience,
                    ValidIssuer = jwtSetting.Issuer
                };

                options.MapInboundClaims = true;
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Cookies["accessToken"]
                                          ?? context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("admin", p => p.RequireClaim(ClaimTypes.Role, "admin"))
            .AddPolicy("user", p => p.RequireClaim(ClaimTypes.Role, "user"))
            .AddPolicy("baseRole", p => p.RequireClaim(ClaimTypes.Role, "user", "admin"));

        return services;
    }
}