using be.Domain.Enums;

namespace be.Application.Dtos.Queries.Account;

public class Account4Dto
{
    public RoleEnum Role { get; set; }
    public short PrivateAccountId { get; set; }
    public string PublicUserId { get; set; } = null!;
}