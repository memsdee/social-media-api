using be.Domain.Attributes;
using be.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace be.Domain.Documents;

[BsonCollectionAttribute.BsonCollection("notifications")]
public class NotificationDocument
{
    [BsonId] public ObjectId Id { get; set; }

    public short SequenceId { get; set; }
    public short ReceiveSequenceId { get; set; }
    public short SenderSequenceId { get; set; }
    public Guid? ThumbnailNoti { get; set; }
    public short? PostSequenceId { get; set; }
    public Guid? PostPublicId { get; set; }
    public short? CmtSequenceId { get; set; }
    public Guid? CmtPublicId { get; set; }
    public string? PreviewContent { get; set; }
    public NotiTargetEnum Target { get; set; }
    public NotiActionEnum Action { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public DateTimeOffset CreateAt { get; set; }
}