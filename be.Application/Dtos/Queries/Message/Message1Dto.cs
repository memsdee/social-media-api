namespace be.Application.Dtos.Queries.Message;

public class Message1Dto
{
    public Guid ConversationId { get; set; }
    public string? Content { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public string SenderPublicId { get; set; } = null!;
    public short Sequence { get; set; }
}