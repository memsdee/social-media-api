using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class ScoreSetting
{
    [Required(ErrorMessage = "Score Light đang rỗng")]
    public PenaltyLevel Light { get; set; } = null!;

    [Required(ErrorMessage = "Score Medium đang rỗng")]
    public PenaltyLevel Medium { get; set; } = null!;

    [Required(ErrorMessage = "Score Severe đang rỗng")]
    public PenaltyLevel Severe { get; set; } = null!;
}