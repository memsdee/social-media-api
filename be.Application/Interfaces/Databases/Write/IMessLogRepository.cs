using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface IMessLogRepository
{
    Task AddAsync(MessageRead input, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<MessageRead> inputs, CancellationToken cancellationToken);
}