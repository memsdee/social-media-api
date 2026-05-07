using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.User;

namespace be.Application.Interfaces.Databases.Read;

public interface IUserReadRepository
{
    Task<User3Dto?> GetUser3Async(string publicUserId, CancellationToken ct);

    Task<User4Dto?> GetUser4Async(string targetPublicUserId, string myPublicUserId, bool isMyProfile,
        CancellationToken ct);

    Task UpdateAvatarAsync(short privateUserId, Guid avatar, CancellationToken ct);
    Task MarkDeletedAsync(short privateUserId, CancellationToken ct);

    Task<CursorResult<UserSearchDto, CursorPayload<short>?>> SearchUsersByNameAsync(string keyword, int limit,
        CursorPayload<short>? cursor, CancellationToken ct);
}