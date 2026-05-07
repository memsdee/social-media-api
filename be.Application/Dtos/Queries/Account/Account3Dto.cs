namespace be.Application.Dtos.Queries.Account;

public class Account3Dto
{
    public bool IsThirdParty { get; set; }
    public string? Pass { get; set; }
    public short PrivateAccountId { get; set; }
    public string PublicUserId { get; set; } = null!;
}