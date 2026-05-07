using be.Application.Interfaces.Databases.Write;
using be.Infrastructure.Database;

namespace be.Infrastructure.Repository.Write;

public class UnitOfWork(WriteContext dbContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(ct);
        return new EfTransaction(transaction);
    }
}