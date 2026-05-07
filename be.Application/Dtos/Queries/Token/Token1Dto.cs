namespace be.Application.Dtos.Queries.Token;

public class Token1Dto
{
    public DateTimeOffset ExpiresAt { get; set; }
    public short PrivateAccountId { get; set; }
}