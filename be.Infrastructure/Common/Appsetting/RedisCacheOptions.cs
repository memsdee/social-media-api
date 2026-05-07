using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class RedisCacheOptions
{
    [Required(ErrorMessage = "RedisCacheOptions Configuration đang trống")]
    public string Configuration { get; set; } = null!;

    [Required(ErrorMessage = "RedisCacheOptions InstanceName đang trống")]
    public string InstanceName { get; set; } = null!;
}