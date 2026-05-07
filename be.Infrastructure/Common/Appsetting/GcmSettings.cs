using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class GcmSettings
{
    [Required(ErrorMessage = "GCM Key đang trống")]
    [MinLength(32, ErrorMessage = "GCM Key < 32)")]
    public string Key { get; set; } = null!;
}