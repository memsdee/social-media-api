namespace be.Application.Dtos.Queries.User;

public class User5Dto
{
    public Guid? Avatar { get; set; }
    public short PrivateUserId { get; set; }
    public string PublicUserId { get; set; } = null!;
}