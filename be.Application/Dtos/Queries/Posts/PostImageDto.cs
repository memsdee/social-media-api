using be.Domain.Enums;

namespace be.Application.Dtos.Queries.Posts;

public class PostImageDto
{
    public short SequenceId { get; set; }
    public Guid PublicId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public PostImages[] Images { get; set; } = [];
}

public class PostImages
{
    public Guid Image { get; set; }
    public ImageEnum Type { get; set; }
}