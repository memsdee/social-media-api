using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Domain.Enums;
using be.Infrastructure.Database;
using MongoDB.Driver;

namespace be.Infrastructure.Repository.Read;

public class ReactReadRepository(ReadContext dbContext) : IReactReadRepository
{
    public async Task<ReactEnum?> GetReactAsync(Guid postIdPublic, string userPublicId, CancellationToken ct)
    {
        var react = await dbContext.Collection<ReactPostDocument>()
            .Find(x => x.PostIdPublic == postIdPublic && x.UserPublicId == userPublicId)
            .Project(x => x.Type)
            .FirstOrDefaultAsync(ct);

        return react == 0 ? null : react;
    }

    public async Task<Dictionary<Guid, ReactEnum>> GetReactsAsync(IEnumerable<Guid> postIdsPublic, string userPublicId,
        CancellationToken ct)
    {
        var ids = postIdsPublic.Distinct().ToArray();
        if (ids.Length == 0) return [];

        var reacts = await dbContext.Collection<ReactPostDocument>()
            .Find(x => ids.Contains(x.PostIdPublic) && x.UserPublicId == userPublicId)
            .Project(x => new { x.PostIdPublic, x.Type })
            .ToListAsync(ct);

        return reacts.ToDictionary(x => x.PostIdPublic, x => x.Type);
    }
}