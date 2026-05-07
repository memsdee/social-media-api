using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;

namespace be.Infrastructure.Repository.Write;

public class MessageRepository(WriteContext dbContext) : IMessageRepository
{
    public async Task AddAsync(Message input, CancellationToken ct)
    {
        await dbContext.Messages.AddAsync(input, ct);
    }
}