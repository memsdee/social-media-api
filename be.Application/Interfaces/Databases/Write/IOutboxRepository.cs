using be.Domain.Enums;

namespace be.Application.Interfaces.Databases.Write;

public interface IOutboxRepository
{
    Task AddAsync<TEvent>(OutboxTopicEnum topic, TEvent payload, CancellationToken cancellationToken)
        where TEvent : class;
}