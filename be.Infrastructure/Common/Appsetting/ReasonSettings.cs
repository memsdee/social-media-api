using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class ReasonSettings
{
    [Required(ErrorMessage = "Score LightCodes đang rỗng")]
    public short[] LightCodes { get; set; } = null!;

    [Required(ErrorMessage = "Score SevereCodes đang rỗng")]
    public short[] SevereCodes { get; set; } = null!;
}