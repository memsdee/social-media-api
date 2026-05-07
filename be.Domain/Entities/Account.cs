using be.Domain.Enums;

namespace be.Domain.Entities;

public class Account
{
    public short Id { get; set; }
    public string? Pass { get; set; }
    public string? Mail { get; set; }
    public RoleEnum Role { get; set; }
    public short Score { get; set; }
    public bool IsThirdParty { get; set; }
    public DateTimeOffset CreatAt { get; set; }
    public StatusAccountEnum Status { get; set; }

    public virtual User UserNavi { get; set; } = null!;
    public virtual ICollection<ThirdPartyLogin> ThirdPartyLoginsNavi { get; set; } = null!;
    public virtual ICollection<Token> TokensNavi { get; set; } = [];
    public virtual ICollection<AdminResolveReportLog> AdminResolveReportLogsNavi { get; set; } = [];
    public virtual ICollection<AdminDelPostLog> AdminDelPostLogsNavi { get; set; } = [];
    public virtual ICollection<AdminDelAccountLog> AdminDelAccountLogsNavi { get; set; } = [];
}