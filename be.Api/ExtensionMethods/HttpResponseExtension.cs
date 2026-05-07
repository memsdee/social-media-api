using be.Infrastructure.Common.Appsetting;

namespace be.Api.ExtensionMethods;

public static class HttpResponseExtension
{
    public static void SetTokens(this HttpResponse response, string accessToken, string refreshToken,
        JwtSettings jwtSettings)
    {
        response.Cookies.Append("accessToken", accessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddHours(jwtSettings.ExpHours),
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

        response.Cookies.Append("refreshToken", refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(jwtSettings.RefreshExpDays),
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });
    }
}