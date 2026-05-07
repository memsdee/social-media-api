using be.Domain.Enums;

namespace be.Application.Dtos.Queries.Posts;

public class Post1Dto
{
    public Guid IdPublic { get; set; }
    public string Content { get; set; } = null!;
    public short TotalComment { get; set; }
    public short TotalDislike { get; set; }
    public short TotalLike { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public PostAuthor PostAuthor { get; set; } = null!;
    public ReactEnum AuthorReact { get; set; }
    public ReactEnum MyReact { get; set; }
    public PostSearchImageDto[] PostImages { get; set; } = [];
    public short Sequence { get; set; }
    public short Score { get; set; }
}

public class PostSearchImageDto
{
    public ImageEnum Type { get; set; }
    public Guid? Image { get; set; }
    public Guid? Before { get; set; }
    public Guid? After { get; set; }
}

public class PostAuthor
{
    public string PublicUserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public Guid? Avatar { get; set; }
}