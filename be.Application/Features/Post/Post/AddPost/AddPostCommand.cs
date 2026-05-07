using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Post.Post.AddPost;

public class AddPostCommand : IRequest<BaseResponse<PostResponse>>
{
    public string Content
    {
        get;
        set => field = value.Trim();
    } = string.Empty;

    public ImageItem[] Images { get; set; } = [];
}