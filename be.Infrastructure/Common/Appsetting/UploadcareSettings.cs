using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class UploadcareSettings
{
    [Required(ErrorMessage = "Uploadcare PublicKey đang rỗng")]
    public string PublicKey { get; set; } = null!;

    [Required(ErrorMessage = "Uploadcare SecretKey đang rỗng")]
    public string SecretKey { get; set; } = null!;

    [Required(ErrorMessage = "Uploadcare ExpMinute đang rỗng")]
    public int ExpMinute { get; set; }

    [Required(ErrorMessage = "Uploadcare UrlApi đang rỗng")]
    public string UrlApi { get; set; } = null!;

    [Required(ErrorMessage = "Uploadcare UrlBase đang rỗng")]
    public string UrlImg { get; set; } = null!;
}