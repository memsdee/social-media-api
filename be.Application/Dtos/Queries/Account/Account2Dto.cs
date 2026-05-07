using be.Domain.Enums;

namespace be.Application.Dtos.Queries.Account;

public class Account2Dto
{
    public string? Pass { get; set; }
    public RoleEnum Role { get; set; }
    public short PrivateAccountId { get; set; }
    public string PublicUserId { get; set; } = null!;
    public bool IsThirdParty { get; set; }
}