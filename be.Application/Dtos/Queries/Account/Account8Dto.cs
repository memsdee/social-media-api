namespace be.Application.Dtos.Queries.Account;

public class Account8Dto
{
    public bool IsDeleted { get; set; }
    public short PrivateAccountId { get; set; }
    public string? Mail { get; set; }
    public string? Pass { get; set; }
    public bool IsThirdParty { get; set; }
}