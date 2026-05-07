using be.Domain.Enums;

namespace be.Application.Interfaces.Databases.Read;

public interface IReactReadRepository
{
    Task<ReactEnum?> GetReactAsync(Guid postIdPublic, string userPublicId, CancellationToken ct);

    Task<Dictionary<Guid, ReactEnum>> GetReactsAsync(IEnumerable<Guid> postIdsPublic, string userPublicId,
        CancellationToken ct);
}