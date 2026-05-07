namespace be.Application.Dtos.Queries.User;

public class User3Dto
{
    public Guid? Avatar { get; set; }
    public string PublicUserId { get; set; } = null!;
    public string Name { get; set; } = null!;
}