using System.Text.Json;
using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;

namespace be.Infrastructure.Repository.Write;

public class OutboxRepository(WriteContext writeContext) : IOutboxRepository
{
    public async Task AddAsync<TEvent>(OutboxTopicEnum topic, TEvent payload, CancellationToken cancellationToken)
        where TEvent : class
    {
        await writeContext.Outbox.AddAsync(new Outbox
        {
            Topic = topic,
            Payload = JsonSerializer.Serialize(payload)
        }, cancellationToken);
    }
}