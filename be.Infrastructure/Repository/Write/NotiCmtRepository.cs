using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;

namespace be.Infrastructure.Repository.Write;

public class NotiCmtRepository(WriteContext dbContext) : INotiCmtRepository
{
    public async Task AddAsync(NotiCmt notiCmt, CancellationToken ct)
    {
        await dbContext.NotiCmts.AddAsync(notiCmt, ct);
    }
}