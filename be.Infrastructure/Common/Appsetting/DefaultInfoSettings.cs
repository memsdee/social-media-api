using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class DefaultInfoSettings
{
    [Required(ErrorMessage = "AppSetting Avatar đang trống")]
    public Guid Avatar { get; set; }

    [Required(ErrorMessage = "AppSetting DeletedName đang trống")]
    public string DeletedName { get; set; } = null!;

    [Required(ErrorMessage = "AppSetting DeletedAvatar đang trống")]
    public Guid DeletedAvatar { get; set; }
}