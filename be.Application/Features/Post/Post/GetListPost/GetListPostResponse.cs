using be.Application.Dtos.Pagination;

namespace be.Application.Features.Post.Post.GetListPost;

public class GetListPostResponse
{
    public List<PostResponse> Posts { get; set; } = [];
    public PagiResult PageProfile { get; set; } = null!;
}