using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface IMessageRepository
{
    Task AddAsync(Message input, CancellationToken ct);
}