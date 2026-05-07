namespace be.Application.Dtos.EventBus;

public class ConversationCreatedEvent
{
    public short SequenceId { get; set; }
    public Guid ConversationPublicId { get; set; }
    public long KeyPart { get; set; }
    public short CreatorSequenceId { get; set; }
    public string? LastMessage { get; set; }
    public DateTimeOffset LastMessageDate { get; set; }
    public DateTimeOffset LastUpdate { get; set; }
    public DateTimeOffset CreateAt { get; set; }
    public ConversationParticipantEvent[] Participants { get; set; } = [];
}

public class ConversationParticipantEvent
{
    public short UserSequenceId { get; set; }
    public Guid? UserAvatar { get; set; }
    public string UserName { get; set; } = null!;
    public short UnreadCount { get; set; }
}