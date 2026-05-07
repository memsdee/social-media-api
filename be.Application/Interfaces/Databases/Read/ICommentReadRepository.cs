using be.Application.Dtos.Pagination;
using be.Domain.Documents;

namespace be.Application.Interfaces.Databases.Read;

public interface ICommentReadRepository
{
    Task MarkAuthorDeletedAsync(short privateAccountId, CancellationToken ct);
    Task AddAsync(CommentDocument input, CancellationToken ct);
    Task<CommentDocument?> GetByIdPublicAsync(Guid idPublic, CancellationToken ct);
    Task<long> CountByPostSequenceIdAsync(short postSequenceId, CancellationToken ct);

    Task<CursorResult<CommentDocument, CursorPayload<DateTimeOffset>?>> GetPagedByPostSequenceIdAsync(
        short postSequenceId, int limit, CursorPayload<DateTimeOffset>? cursor, CancellationToken ct);
}