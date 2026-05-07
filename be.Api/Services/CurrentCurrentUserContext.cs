using System.Security.Claims;
using be.Application.Interfaces.Security;

namespace be.Api.Services;

public class CurrentCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public string? UserId => httpContextAccessor.HttpContext?
        .User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? RefreshToken => httpContextAccessor.HttpContext?
        .Request.Cookies["refreshToken"];
}