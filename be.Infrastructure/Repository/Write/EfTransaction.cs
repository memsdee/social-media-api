using be.Application.Interfaces.Databases.Write;
using Microsoft.EntityFrameworkCore.Storage;

namespace be.Infrastructure.Repository.Write;

public class EfTransaction(IDbContextTransaction transaction) : ITransaction
{
    public async Task CommitAsync(CancellationToken ct)
    {
        await transaction.CommitAsync(ct);
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        await transaction.RollbackAsync(ct);
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
    }
}