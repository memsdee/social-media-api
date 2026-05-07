using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class KafkaSettings
{
    [Required(ErrorMessage = "KafkaSetting BootstrapServers đang trống")]
    public string BootstrapServers { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;
    public bool UseSaslSsl { get; set; } = false;
}