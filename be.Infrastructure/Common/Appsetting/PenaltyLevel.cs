using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class PenaltyLevel
{
    [Required(ErrorMessage = "Score Post đang rỗng")]
    public int Post { get; set; }

    [Required(ErrorMessage = "Score Account đang rỗng")]
    public int Account { get; set; }
}