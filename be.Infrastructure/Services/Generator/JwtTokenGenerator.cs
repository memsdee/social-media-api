using be.Application.Interfaces.Generator;
using be.Infrastructure.Common.Appsetting;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Services.Generator;

public class JwtTokenGenerator(IOptions<JwtSettings> jwtSetting) : IJwtTokenGenerator
{
    public string Key => jwtSetting.Value.Key;
    public string Issuer => jwtSetting.Value.Issuer;
    public string Audience => jwtSetting.Value.Audience;
    public int ExpHours => jwtSetting.Value.ExpHours;
    public int RefreshExpDays => jwtSetting.Value.RefreshExpDays;
}