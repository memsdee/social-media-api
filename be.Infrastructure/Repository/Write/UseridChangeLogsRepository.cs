using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class UseridChangeLogsRepository(WriteContext dbContext) : IUseridChangeLogsRepository
{
    public async Task<DateTimeOffset?> GetCreateAtByPublicUserId(short pritaveUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.UseridChangeLogs.Where(x => x.UserId == pritaveUserId)
            .Select(x => x.ChangedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(UseridChangeLog input, CancellationToken cancellationToken)
    {
        await dbContext.UseridChangeLogs.AddAsync(input, cancellationToken);
    }
}