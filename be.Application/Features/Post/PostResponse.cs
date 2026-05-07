using be.Domain.Enums;

namespace be.Application.Features.Post;

public class PostResponse
{
    public Guid IdPublic { get; set; }
    public string? Content { get; set; }
    public short TotalComment { get; set; }
    public short TotalLike { get; set; }
    public short TotalDislike { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ReactEnum? IsReact { get; set; }
    public ReactEnum? MyReact { get; set; }
    public PostAuthor PostAuthor { get; set; } = null!;
    public List<PostImage>? Images { get; set; }
}