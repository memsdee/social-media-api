using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;

namespace be.Infrastructure.Repository.Write;

public class MessLogRepository(WriteContext dbContext) : IMessLogRepository
{
    public async Task AddAsync(MessageRead input, CancellationToken cancellationToken)
    {
        await dbContext.MessageReads.AddAsync(input, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<MessageRead> inputs, CancellationToken cancellationToken)
    {
        await dbContext.MessageReads.AddRangeAsync(inputs, cancellationToken);
    }
}