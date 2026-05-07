namespace be.Application.Dtos.Queries.Posts;

public class PostForCommentDto
{
    public short Id { get; set; }
    public short UserId { get; set; }
    public string PostAuthor { get; set; } = null!;
    public Guid? Image { get; set; }
    public string Content { get; set; } = null!;
}