namespace be.Application.Dtos.Queries.Follow;

public class Follow1Dto
{
    public short SequenceId { get; set; }
    public string? PublicUserId { get; set; }
    public string UserName { get; set; } = null!;
    public Guid? Avatar { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}