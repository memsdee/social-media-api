using be.Application.Dtos.Pagination;
using be.Application.Dtos.Queries.User;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Infrastructure.Database;
using be.Infrastructure.Helper;
using MongoDB.Bson;
using MongoDB.Driver;

namespace be.Infrastructure.Repository.Read;

public class UserReadRepository(ReadContext dbContext) : IUserReadRepository
{
    public async Task<User3Dto?> GetUser3Async(string publicUserId, CancellationToken ct)
    {
        return await dbContext.Collection<UserDocument>()
            .Find(x => x.UserIdPublic == publicUserId)
            .Project(x => new User3Dto
            {
                PublicUserId = x.UserIdPublic,
                Avatar = x.Avatar,
                Name = x.Name
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<User4Dto?> GetUser4Async(string targetPublicUserId, string myPublicUserId, bool isMyProfile,
        CancellationToken ct)
    {
        return await dbContext.Collection<UserDocument>()
            .Find(x => x.UserIdPublic == targetPublicUserId && x.IsDeleteAccount == false)
            .Project(x => new User4Dto
            {
                PublicUserId = x.UserIdPublic,
                Name = x.Name,
                Avatar = x.Avatar,
                TotalPost = x.TotalPosts,
                TotalFollower = x.TotalFollowers,
                TotalFollowing = x.TotalFollowing
            }).FirstOrDefaultAsync(ct);
    }

    public async Task UpdateAvatarAsync(short privateUserId, Guid avatar, CancellationToken ct)
    {
        await dbContext.Collection<UserDocument>()
            .UpdateOneAsync(x => x.SequenceId == privateUserId,
                Builders<UserDocument>.Update.Set(x => x.Avatar, avatar), cancellationToken: ct);
    }

    public async Task MarkDeletedAsync(short privateUserId, CancellationToken ct)
    {
        await dbContext.Collection<UserDocument>()
            .UpdateOneAsync(x => x.AccountSequenceId == privateUserId,
                Builders<UserDocument>.Update.Set(x => x.IsDeleteAccount, true), cancellationToken: ct);
    }

    public async Task<CursorResult<UserSearchDto, CursorPayload<short>?>> SearchUsersByNameAsync(string keyword,
        int limit,
        CursorPayload<short>? cursor, CancellationToken ct)
    {
        var filter = Builders<UserDocument>.Filter.And(
            Builders<UserDocument>.Filter.Regex(x => x.Name, new BsonRegularExpression(keyword, "i")),
            Builders<UserDocument>.Filter.Eq(x => x.IsDeleteAccount, false),
            ReadCursorPagiFilterHelper.BuildCursorFilter<UserDocument, short>(
                x => x.TotalFollowers, x => x.SequenceId, cursor?.Selector, cursor?.Id)
        );

        var items = await dbContext.Collection<UserDocument>()
            .Find(filter)
            .SortByDescending(x => x.TotalFollowers)
            .ThenByDescending(x => x.SequenceId)
            .Limit(limit + 1)
            .Project(x => new UserSearchDto
            {
                SequenceId = x.SequenceId,
                PublicUserId = x.UserIdPublic,
                Name = x.Name,
                Avatar = x.Avatar,
                TotalFollowers = x.TotalFollowers
            })
            .ToListAsync(ct);

        return ReadCursorPagiCaculHelper.Paginate(
            items,
            limit,
            x => new CursorPayload<short>(x.TotalFollowers, x.SequenceId));
    }
}