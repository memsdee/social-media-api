using be.Domain.Entities;
using be.Domain.Enums;

namespace be.Application.Interfaces.Databases.Write;

public interface IReactPostRepository
{
    Task AddAsync(ReactPost reactPost, CancellationToken ct);
    void Delete(ReactPost reactPost);
    Task<ReactPost?> GetByUserAndPostAsync(short userId, short postId, CancellationToken ct);
    Task<Dictionary<ReactEnum, short>> GetTotalReactsAsync(short postId, CancellationToken ct);
}