namespace be.Application.Dtos.Queries.Conversation;

public class Conversation1Dto
{
    public short SequenceId { get; set; }
    public Guid ConversationPublicId { get; set; }
    public string UserName { get; set; } = null!;
    public Guid? Avatar { get; set; }
    public string? LastMessage { get; set; } = null!;
    public DateTimeOffset LastMessageDate { get; set; }
    public DateTimeOffset LastUpdate { get; set; }
    public short UnreadCount { get; set; }
}