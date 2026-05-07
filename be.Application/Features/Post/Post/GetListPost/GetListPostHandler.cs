using System.Text.Json;
using be.Application.Common.Constants;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Posts;
using be.Application.Dtos.Shared;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using be.Domain;
using MediatR;

namespace be.Application.Features.Post.Post.GetListPost;

public class GetListPostHandler(
    IPostReadRepository postReadRepository,
    IFollowReadRepository followReadRepository,
    IFormat format,
    ICurrentUserContext currentUserContext,
    IEncryption encryption)
    : IRequestHandler<GetListPostQuery, BaseResponse<GetListPostResponse>>
{
    public async Task<BaseResponse<GetListPostResponse>> Handle(GetListPostQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.UserId;
        var isLogin = currentUserId is not null;

        if (!isLogin && (request.Tab == TabPeriod.Following || request.Tab == TabPeriod.Latest))
            throw new CustomException.UnauthorizedException("Vui lòng đăng nhập để xem tab này");

        HashSet<string>? followingIds = null;

        if (request.Tab == TabPeriod.Following)
        {
            followingIds = await followReadRepository.GetAllFolloweeIdPublicsAsync(currentUserId!, cancellationToken);
            if (followingIds.Count == 0)
                return new BaseResponse<GetListPostResponse>
                {
                    Data = new GetListPostResponse
                    {
                        Posts = [],
                        PageProfile = new PagiResult
                        {
                            HasNextPage = false,
                            NextCursor = null
                        }
                    }
                };
        }

        var isDateSorted = request.Tab == TabPeriod.Following || request.Tab == TabPeriod.Latest;

        bool hasNextPage;
        string? nextCursorStr;
        IEnumerable<Post1Dto> items;

        if (isDateSorted)
        {
            var cursor = string.IsNullOrWhiteSpace(request.Cursor)
                ? null
                : JsonSerializer.Deserialize<CursorPayload<DateTimeOffset>>(encryption.Decrypt(request.Cursor));

            var result = await postReadRepository.GetListPostByDateAsync(
                request.TargetId, followingIds, request.Limit, cursor, cancellationToken);

            hasNextPage = result.HasNextPage;
            nextCursorStr = result.NextCursor is null
                ? null
                : encryption.Encrypt(JsonSerializer.Serialize(result.NextCursor));
            items = result.Items;
        }
        else
        {
            var cursor = string.IsNullOrWhiteSpace(request.Cursor)
                ? null
                : JsonSerializer.Deserialize<CursorPayload<short>>(encryption.Decrypt(request.Cursor));

            var result = await postReadRepository.GetListPostByScoreAsync(
                request.TargetId, request.Limit, cursor, cancellationToken);

            hasNextPage = result.HasNextPage;
            nextCursorStr = result.NextCursor is null
                ? null
                : encryption.Encrypt(JsonSerializer.Serialize(result.NextCursor));
            items = result.Items;
        }

        var followedAuthorIds = isLogin
            ? await followReadRepository.GetFolloweeIdSetAsync(
                currentUserId!,
                items.Select(x => x.PostAuthor.PublicUserId),
                cancellationToken)
            : [];

        var posts = items.Select(x => new PostResponse
        {
            IdPublic = x.IdPublic,
            Content = x.Content,
            TotalComment = x.TotalComment,
            TotalDislike = x.TotalDislike,
            TotalLike = x.TotalLike,
            CreatedAt = x.CreatedAt,
            PostAuthor = new PostAuthor
            {
                UserId = x.PostAuthor.PublicUserId,
                UserName = x.PostAuthor.Name,
                UserAvatar = format.FormatImageUrl(x.PostAuthor.Avatar, x.PostAuthor.PublicUserId),
                IsFollow = isLogin && followedAuthorIds.Contains(x.PostAuthor.PublicUserId)
            },
            IsReact = null,
            MyReact = null,
            Images = x.PostImages
                .Select(i => new PostImage
                {
                    Type = i.Type,
                    Image = i.Image.HasValue
                        ? format.FormatImageUrl(i.Image.Value, x.PostAuthor.PublicUserId)
                        : string.Empty,
                    Before = i.Before.HasValue
                        ? format.FormatImageUrl(i.Before.Value, x.PostAuthor.PublicUserId)
                        : null,
                    After = i.After.HasValue
                        ? format.FormatImageUrl(i.After.Value, x.PostAuthor.PublicUserId)
                        : null
                })
                .ToList()
        }).ToList();

        return new BaseResponse<GetListPostResponse>
        {
            Data = new GetListPostResponse
            {
                Posts = posts,
                PageProfile = new PagiResult
                {
                    HasNextPage = hasNextPage,
                    NextCursor = nextCursorStr
                }
            }
        };
    }
}