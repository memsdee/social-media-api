namespace be.Application.Dtos.EventBus;

public class CommentCreatedEvent
{
    public short SequenceId { get; set; }
    public Guid IdPublic { get; set; }
    public short PostSequenceId { get; set; }
    public Guid PostPublicId { get; set; }
    public short UserSequenceId { get; set; }
    public string UserIdPublic { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public Guid? UserAvatar { get; set; }
    public string Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}