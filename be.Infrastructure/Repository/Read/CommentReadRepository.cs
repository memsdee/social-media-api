using be.Application.Dtos.Pagination;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Infrastructure.Database;
using be.Infrastructure.Helper;
using MongoDB.Driver;

namespace be.Infrastructure.Repository.Read;

public class CommentReadRepository(ReadContext dbContext) : ICommentReadRepository
{
    public async Task AddAsync(CommentDocument input, CancellationToken ct)
    {
        await dbContext.Collection<CommentDocument>()
            .InsertOneAsync(input, null, ct);
    }

    public async Task MarkAuthorDeletedAsync(short privateAccountId, CancellationToken ct)
    {
        await dbContext.Collection<CommentDocument>()
            .UpdateManyAsync(
                x => x.UserSquenceId == privateAccountId,
                Builders<CommentDocument>.Update.Set(x => x.IsDeleteAccount, true),
                cancellationToken: ct);
    }

    public async Task<CommentDocument?> GetByIdPublicAsync(Guid idPublic, CancellationToken ct)
    {
        return await dbContext.Collection<CommentDocument>()
            .Find(x => x.IdPublic == idPublic)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<long> CountByPostSequenceIdAsync(short postSequenceId, CancellationToken ct)
    {
        return await dbContext.Collection<CommentDocument>()
            .CountDocumentsAsync(x => x.PostSquenceId == postSequenceId, cancellationToken: ct);
    }

    public async Task<CursorResult<CommentDocument, CursorPayload<DateTimeOffset>?>> GetPagedByPostSequenceIdAsync(
        short postSequenceId, int limit, CursorPayload<DateTimeOffset>? cursor, CancellationToken ct)
    {
        var filter = Builders<CommentDocument>.Filter.And(
            Builders<CommentDocument>.Filter.Eq(x => x.PostSquenceId, postSequenceId),
            ReadCursorPagiFilterHelper.BuildCursorFilter<CommentDocument, DateTimeOffset>(
                x => x.CreateAt, x => x.SequenceId, cursor?.Selector, cursor?.Id)
        );

        var items = await dbContext.Collection<CommentDocument>()
            .Find(filter)
            .SortByDescending(x => x.CreateAt)
            .ThenByDescending(x => x.SequenceId)
            .Limit(limit + 1)
            .ToListAsync(ct);

        return ReadCursorPagiCaculHelper.Paginate(
            items, limit,
            x => new CursorPayload<DateTimeOffset>(x.CreateAt, x.SequenceId));
    }
}