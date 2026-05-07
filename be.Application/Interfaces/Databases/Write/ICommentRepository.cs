using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface ICommentRepository
{
    Task AddAsync(Comment comment, CancellationToken ct);
    Task<int> CountByPostIdAsync(short postId, CancellationToken ct);
    Task<Comment?> GetByIdPublicAsync(Guid idPublic, CancellationToken ct);
    void Delete(Comment comment);
    Task<List<short>> GetNotiIdsByCommentIdAsync(short commentId, CancellationToken ct);
}