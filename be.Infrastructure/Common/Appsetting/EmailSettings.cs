using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class EmailSettings
{
    [Required(ErrorMessage = "EmailSettings đang trống")]
    public string SenderName { get; set; } = null!;

    [Required(ErrorMessage = "EmailSettings đang trống")]
    public string SenderEmail { get; set; } = null!;

    [Required(ErrorMessage = "EmailSettings đang trống")]
    public string SmtpServer { get; set; } = null!;

    public int SmtpPort { get; set; }

    [Required(ErrorMessage = "Smtp Login đang trống")]
    public string SmtpUser { get; set; } = null!;

    [Required(ErrorMessage = "EmailSettings đang trống")]
    public string SmtpPassword { get; set; } = null!;
}