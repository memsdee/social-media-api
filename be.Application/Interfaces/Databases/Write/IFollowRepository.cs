using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface IFollowRepository
{
    Task AddAsync(Follow input, CancellationToken ct);
    Task<int> UnFollowAsync(short privateFollowerId, short privateFolloweedId, CancellationToken ct);
}