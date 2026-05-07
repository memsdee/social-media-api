namespace be.Application.Dtos.Queries.User;

public class User4Dto
{
    public string PublicUserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid? Avatar { get; set; }
    public short TotalPost { get; set; }
    public short TotalFollower { get; set; }
    public short TotalFollowing { get; set; }
}