using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface INotiReactPostRepository
{
    Task AddAsync(NotiReactPost notiReactPost, CancellationToken ct);
    Task<List<short>> GetNotiIdsByPostIdAsync(short postId, CancellationToken ct);
    Task<NotiReactPost?> GetByIdAsync(short notiId, CancellationToken ct);
}