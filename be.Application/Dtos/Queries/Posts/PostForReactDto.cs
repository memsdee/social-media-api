using be.Domain.Enums;

namespace be.Application.Dtos.Queries.Posts;

public class PostForReactDto
{
    public short Id { get; set; }
    public short UserId { get; set; }
    public string PostAuthorPublicId { get; set; } = null!;
    public Guid? Thumbnail { get; set; }
    public string? Content { get; set; }
    public ReactEnum? AuthorReact { get; set; }
}