namespace be.Application.Dtos.Queries.User;

public class User2Dto
{
    public string PublicUserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid? Avatar { get; set; }
    public bool IsDeleteAccount { get; set; }
    public short SequenceId { get; set; }
}