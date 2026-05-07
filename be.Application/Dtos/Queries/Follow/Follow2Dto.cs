namespace be.Application.Dtos.Queries.Follow;

public class Follow2Dto
{
    public string? UserId { get; set; }
    public string UserName { get; set; } = null!;
    public Guid? Avatar { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public short Sequence { get; set; }
}