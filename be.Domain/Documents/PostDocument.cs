using be.Domain.Attributes;
using be.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace be.Domain.Documents;

[BsonCollectionAttribute.BsonCollection("post")]
public class PostDocument
{
    [BsonId] public ObjectId Id { get; set; }

    public short SequenceId { get; set; }
    public Guid IdPublic { get; set; }
    public short UserSequenceId { get; set; }
    public string UserIdPublic { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public Guid? UserAvatar { get; set; }
    public bool IsDeleteAccount { get; set; }
    public string? Content { get; set; }
    public List<PostImageReadModel> Images { get; set; } = [];
    public short TotalCmt { get; set; }
    public short TotalLike { get; set; }
    public short TotalDislike { get; set; }
    public DateTimeOffset CreateAt { get; set; }
    public short ScoreTrend { get; set; }
    public short ScoreReport { get; set; }
    public StatusPostEnum Status { get; set; }
}

public class PostImageReadModel
{
    public short ImageSquenceId { get; set; }
    public Guid Image { get; set; }

    [BsonRepresentation(BsonType.String)] public ImageEnum ImageType { get; set; }

    public short? ImageGroupId { get; set; }
}