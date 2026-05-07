using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Follow;
using be.Domain.Documents;

namespace be.Application.Interfaces.Databases.Read;

public interface IFollowReadRepository
{
    Task AddAsync(FollowDocument input, CancellationToken ct);

    Task<CursorResult<Follow2Dto, CursorPayload<DateTimeOffset>?>> GetListFollowerAsync(short privateUserId, int limit,
        CursorPayload<DateTimeOffset>? cursor, CancellationToken ctx);

    Task<CursorResult<Follow2Dto, CursorPayload<DateTimeOffset>?>> GetListFolloweeAsync(short privateUserId, int limit,
        CursorPayload<DateTimeOffset>? cursor, CancellationToken ctx);

    Task UnfollowAsync(short followerSequence, short followeeSequence, CancellationToken ct);
    Task<bool> IsFollowingAsync(string publicFollowerId, string publicFolloweedId, CancellationToken ct);

    Task<HashSet<string>> GetFolloweeIdSetAsync(string publicFollowerId, IEnumerable<string> followeePublicIds,
        CancellationToken ct);

    Task<HashSet<string>> GetAllFolloweeIdPublicsAsync(string publicFollowerId, CancellationToken ct);
}