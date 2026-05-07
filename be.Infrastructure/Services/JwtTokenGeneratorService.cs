using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using be.Application.Dtos.Shared;
using be.Application.Interfaces.Security;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Domain.Helpers;
using be.Infrastructure.Common.Appsetting;
using be.Infrastructure.Database;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace be.Infrastructure.Services;

public class JwtTokenGeneratorService(IOptions<JwtSettings> jwtSetting, WriteContext dbContext) : ITokenGenerator
{
    private readonly JwtSettings _jwtSetting = jwtSetting.Value;

    public Task<BaseResponse<string>> CreateAccessTokensAsync(string userId, RoleEnum role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Role, role.ToString())
        };

        var tokenDesc = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            Expires = DateTime.UtcNow.AddHours(_jwtSetting.ExpHours),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDesc);
        var jwtToken = tokenHandler.WriteToken(token);

        return Task.FromResult(new BaseResponse<string>
        {
            Data = jwtToken
        });
    }

    public async Task<BaseResponse<string>> CreateRefreshTokenAsync(short idAccount, CancellationToken cancellation)
    {
        var refreshToken = Guid.NewGuid().ToString().ToLower();
        var hashRefreshToken = HashHelper.GetHash(refreshToken);

        var model = new Token
        {
            RefreshToken = hashRefreshToken,
            AccountId = idAccount,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSetting.RefreshExpDays)
        };

        dbContext.Tokens.Add(model);
        await dbContext.SaveChangesAsync(cancellation);

        return new BaseResponse<string>
        {
            Data = refreshToken
        };
    }
}