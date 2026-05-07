using System.Text.Json;
using be.Application.Dtos.Pagination;
using be.Application.Dtos.Shared;
using be.Application.Features.Post;
using be.Application.Interfaces.Databases.Read;
using be.Application.Interfaces.Security;
using be.Application.Interfaces.Services;
using MediatR;

namespace be.Application.Features.Search.SearchAll;

public class SearchAllHandler(
    IUserReadRepository userReadRepository,
    IPostReadRepository postReadRepository,
    IFollowReadRepository followReadRepository,
    ICurrentUserContext currentUserContext,
    IFormat format,
    IEncryption encryption
)
    : IRequestHandler<SearchAllQuery, BaseResponse<SearchAllResponse>>
{
    public async Task<BaseResponse<SearchAllResponse>> Handle(SearchAllQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = currentUserContext.UserId;
        var isLogin = currentUserId is not null;

        var userTask = userReadRepository.SearchUsersByNameAsync(request.Q, request.LimitUser, null, cancellationToken);
        var postTask =
            postReadRepository.GetListPostByContentAsync(request.Q, request.LimitPost, null, cancellationToken);
        await Task.WhenAll(userTask, postTask);

        var resultAccount = await userTask;
        var resultPost = await postTask;

        var allAuthorPublicIds = resultAccount.Items.Select(x => x.PublicUserId)
            .Concat(resultPost.Items.Select(x => x.PostAuthor.PublicUserId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var followedAuthorPublicIds = isLogin
            ? await followReadRepository.GetFolloweeIdSetAsync(currentUserId!, allAuthorPublicIds, cancellationToken)
            : [];

        var users = resultAccount.Items.Select(x => new UerSearchDto
        {
            UserId = x.PublicUserId,
            UserName = x.Name,
            Avatar = format.FormatImageUrl(x.Avatar, x.PublicUserId),
            TotalFollower = x.TotalFollowers,
            IsFollowing = isLogin && followedAuthorPublicIds.Contains(x.PublicUserId)
        }).ToList();

        var posts = resultPost.Items.Select(x =>
        {
            return new PostResponse
            {
                IdPublic = x.IdPublic,
                Content = x.Content,
                TotalComment = x.TotalComment,
                CreatedAt = x.CreatedAt,
                TotalLike = x.TotalLike,
                TotalDislike = x.TotalDislike,
                PostAuthor = new PostAuthor
                {
                    UserId = x.PostAuthor.PublicUserId,
                    UserName = x.PostAuthor.Name,
                    UserAvatar = format.FormatImageUrl(x.PostAuthor.Avatar, x.PostAuthor.PublicUserId),
                    IsFollow = isLogin && followedAuthorPublicIds.Contains(x.PostAuthor.PublicUserId)
                },
                IsReact = x.AuthorReact,
                MyReact = isLogin ? x.MyReact : null,
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
            };
        }).ToList();

        return new BaseResponse<SearchAllResponse>
        {
            Data = new SearchAllResponse
            {
                Post = posts,
                User = users,
                PageProfilePost = new PagiResult
                {
                    HasNextPage = resultPost.HasNextPage,
                    NextCursor = resultPost.NextCursor is null
                        ? null
                        : encryption.Encrypt(JsonSerializer.Serialize(resultPost.NextCursor))
                },
                PageProfileUser = new PagiResult
                {
                    HasNextPage = resultAccount.HasNextPage,
                    NextCursor = resultAccount.NextCursor is null
                        ? null
                        : encryption.Encrypt(JsonSerializer.Serialize(resultAccount.NextCursor))
                }
            }
        };
    }
}