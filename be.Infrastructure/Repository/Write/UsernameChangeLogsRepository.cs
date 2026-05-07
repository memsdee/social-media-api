using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class UsernameChangeLogsRepository(WriteContext dbContext) : IUsernameChangeLogsRepository
{
    public async Task<DateTimeOffset?> GetCreateAtByPritaveUserId(short pritaveUserId,
        CancellationToken cancellationToken)
    {
        return await dbContext.UsernameChangeLogs
            .Where(x => x.UserId == pritaveUserId)
            .Select(x => x.ChangeAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(UsernameChangeLog input, CancellationToken cancellationToken)
    {
        await dbContext.UsernameChangeLogs.AddAsync(input, cancellationToken);
    }
}