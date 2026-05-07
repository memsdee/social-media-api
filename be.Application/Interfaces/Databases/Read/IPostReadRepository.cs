using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Posts;
using be.Domain.Documents;
using be.Domain.Enums;

namespace be.Application.Interfaces.Databases.Read;

public interface IPostReadRepository
{
    Task UpdateStatusAsync(Guid publicId, StatusPostEnum status, CancellationToken ctx);
    Task AddAsync(PostDocument document, CancellationToken ct);
    Task MarkAuthorDeletedAsync(short privateAccountId, CancellationToken ct);

    Task<CursorResult<PostImageDto, CursorPayload<DateTimeOffset>?>> GetPostImageAsync(short privateUserId, int limit,
        CursorPayload<DateTimeOffset>? cursor, CancellationToken ct);

    Task<CursorResult<Post1Dto, CursorPayload<short>?>> GetListPostByContentAsync(string content, int limit,
        CursorPayload<short>? cursor, CancellationToken ct);

    Task<CursorResult<Post1Dto, CursorPayload<DateTimeOffset>?>> GetListPostByDateAsync(string? targetIdPublic,
        HashSet<string>? followingUserIdsPublic, int limit, CursorPayload<DateTimeOffset>? cursor,
        CancellationToken ct);

    Task<CursorResult<Post1Dto, CursorPayload<short>?>> GetListPostByScoreAsync(string? targetIdPublic, int limit,
        CursorPayload<short>? cursor, CancellationToken ct);

    Task<PostDocument?> GetByPublicIdAsync(Guid publicId, CancellationToken ct);
}