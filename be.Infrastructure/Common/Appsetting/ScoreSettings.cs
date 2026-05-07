using System.ComponentModel.DataAnnotations;

namespace be.Infrastructure.Common.Appsetting;

public class ScoreSettings
{
    [Required(ErrorMessage = "ScoreReport đang rỗng")]
    public ScoreSetting Score { get; set; } = null!;

    [Required(ErrorMessage = "ScoreReportReason đang rỗng")]
    public ReasonSettings Reason { get; set; } = null!;

    [Required(ErrorMessage = "ScoreDeletePost đang rỗng")]
    public int DeletePost { get; set; }

    [Required(ErrorMessage = "ScoreDeleteAccount đang rỗng")]
    public int DeleteAccount { get; set; }
}