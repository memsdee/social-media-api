namespace be.Application.Dtos.Queries.User;

public class UserSearchDto
{
    public short SequenceId { get; set; }
    public string PublicUserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid? Avatar { get; set; }
    public short TotalFollowers { get; set; }
}