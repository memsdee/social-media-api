using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Post.Post.GetListPost;

public record GetListPostQuery(string? TargetId, string? Cursor, short Limit, string? Tab)
    : IRequest<BaseResponse<GetListPostResponse>>
{
}