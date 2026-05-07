namespace be.Application.Interfaces.Databases.Write;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<ITransaction> BeginTransactionAsync(CancellationToken ct);
}