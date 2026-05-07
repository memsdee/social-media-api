using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.Follow;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Infrastructure.Common.Appsetting;
using be.Infrastructure.Database;
using be.Infrastructure.Helper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace be.Infrastructure.Repository.Read;

public class FollowReadRepository(ReadContext dbContext, IOptions<DefaultInfoSettings> defInfoSetting)
    : IFollowReadRepository
{
    public async Task AddAsync(FollowDocument input, CancellationToken ct)
    {
        await dbContext.Collection<FollowDocument>()
            .InsertOneAsync(input, null, ct);
    }

    public async Task UnfollowAsync(short followerSequence, short followeeSequence, CancellationToken ct)
    {
        await dbContext.Collection<FollowDocument>()
            .DeleteOneAsync(
                Builders<FollowDocument>.Filter.And(
                    Builders<FollowDocument>.Filter.Eq(x => x.FollowerSequenceId, followerSequence),
                    Builders<FollowDocument>.Filter.Eq(x => x.FolloweeSequenceId, followeeSequence)
                ),
                null, ct);
    }

    public async Task<CursorResult<Follow2Dto, CursorPayload<DateTimeOffset>?>> GetListFollowerAsync(
        short privateUserId, int limit,
        CursorPayload<DateTimeOffset>? cursor,
        CancellationToken ct)
    {
        var filter = Builders<FollowDocument>.Filter.And(
            Builders<FollowDocument>.Filter.Eq(x => x.FolloweeSequenceId, privateUserId),
            ReadCursorPagiFilterHelper.BuildCursorFilter<FollowDocument, DateTimeOffset>(
                x => x.CreatedAt, x => x.Sequence, cursor?.Selector, cursor?.Id)
        );

        var items = await dbContext.Collection<FollowDocument>()
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Sequence)
            .Limit(limit + 1)
            .Project(x => new Follow2Dto
            {
                Sequence = x.Sequence,
                UserId = x.FollowerIsDeleteAccount ? null : x.FollowerIdPublic,
                UserName = x.FollowerIsDeleteAccount ? defInfoSetting.Value.DeletedName : x.FollowerName,
                Avatar = x.FollowerIsDeleteAccount ? defInfoSetting.Value.Avatar : x.FollowerAvatar,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return ReadCursorPagiCaculHelper.Paginate(
            items, limit,
            x => new CursorPayload<DateTimeOffset>(x.CreatedAt, x.Sequence));
    }

    public async Task<CursorResult<Follow2Dto, CursorPayload<DateTimeOffset>?>> GetListFolloweeAsync(
        short privateUserId, int limit,
        CursorPayload<DateTimeOffset>? cursor,
        CancellationToken ct)
    {
        var filter = Builders<FollowDocument>.Filter.And(
            Builders<FollowDocument>.Filter.Eq(x => x.FollowerSequenceId, privateUserId),
            ReadCursorPagiFilterHelper.BuildCursorFilter<FollowDocument, DateTimeOffset>(
                x => x.CreatedAt, x => x.Sequence, cursor?.Selector, cursor?.Id)
        );

        var items = await dbContext.Collection<FollowDocument>()
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ThenBy(x => x.Sequence)
            .Limit(limit + 1)
            .Project(x => new Follow2Dto
            {
                Sequence = x.Sequence,
                UserId = x.FolloweeIsDeleteAccount ? null : x.FolloweeIdPublic,
                UserName = x.FolloweeIsDeleteAccount ? defInfoSetting.Value.DeletedName : x.FolloweeName,
                Avatar = x.FolloweeIsDeleteAccount ? defInfoSetting.Value.Avatar : x.FolloweeAvatar,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return ReadCursorPagiCaculHelper.Paginate(
            items, limit,
            x => new CursorPayload<DateTimeOffset>(x.CreatedAt, x.Sequence)
        );
    }

    public async Task<bool> IsFollowingAsync(string publicFollowerId, string publicFolloweedId, CancellationToken ct)
    {
        return await dbContext.Collection<FollowDocument>()
            .Find(x => x.FollowerIdPublic == publicFollowerId && x.FolloweeIdPublic == publicFolloweedId)
            .AnyAsync(ct);
    }

    public async Task<HashSet<string>> GetFolloweeIdSetAsync(string publicFollowerId,
        IEnumerable<string> followeePublicIds,
        CancellationToken ct)
    {
        var followeeIds = followeePublicIds.Distinct().ToArray();
        if (followeeIds.Length == 0)
            return [];

        var matched = await dbContext.Collection<FollowDocument>()
            .Find(x => x.FollowerIdPublic == publicFollowerId && followeeIds.Contains(x.FolloweeIdPublic))
            .Project(x => x.FolloweeIdPublic)
            .ToListAsync(ct);

        return matched.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<HashSet<string>> GetAllFolloweeIdPublicsAsync(string publicFollowerId, CancellationToken ct)
    {
        var matched = await dbContext.Collection<FollowDocument>()
            .Find(x => x.FollowerIdPublic == publicFollowerId)
            .Project(x => x.FolloweeIdPublic)
            .ToListAsync(ct);

        return matched.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}