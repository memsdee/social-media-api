using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class JwtSettings
{
    [Required(ErrorMessage = "JwtKey đang rỗng")]
    [MinLength(32, ErrorMessage = "JWT Key < 32)")]
    public string Key { get; set; } = null!;

    [Required(ErrorMessage = "JwtIssuer đang rỗng")]
    public string Issuer { get; set; } = null!;

    [Required(ErrorMessage = "JwtAudience đang rỗng")]
    public string Audience { get; set; } = null!;

    [Required(ErrorMessage = "JwtExp đang rỗng")]
    public int ExpHours { get; set; }

    [Required(ErrorMessage = "RefreshExpDays đang rỗng")]
    public int RefreshExpDays { get; set; }
}