using be.Domain.Enums;

namespace be.Domain.Entities;

public class Outbox
{
    public int Id { get; set; }
    public OutboxTopicEnum Topic { get; set; }
    public string Payload { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}