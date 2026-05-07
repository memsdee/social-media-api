using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class GoogleSettings
{
    [Required(ErrorMessage = "OauthSetting Google ClientId đang trống")]
    public string ClientId { get; set; } = null!;

    [Required(ErrorMessage = "OauthSetting Google ClientSecret đang trống")]
    public string ClientSecret { get; set; } = null!;

    [Required(ErrorMessage = "OauthSetting Google RedirectUri đang trống")]
    public string RedirectUri { get; set; } = null!;
}