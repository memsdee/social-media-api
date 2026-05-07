using be.Domain.Enums;

namespace be.Application.Dtos.EventBus;

public class PostEvent
{
    public short SequenceId { get; set; }
    public Guid PublicId { get; set; }
    public short UserSequenceId { get; set; }
    public string UserPublicId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public StatusPostEnum Status { get; set; }
    public List<PostImageEvent> Images { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsDelete { get; set; }
}

public class PostImageEvent
{
    public Guid Image { get; set; }
    public ImageEnum ImageType { get; set; }
    public short? ImageGroupId { get; set; }
}