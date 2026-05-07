using be.Application.Dtos.EventBus;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Enums;
using be.Infrastructure.Common.Appsetting;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Kafka.Consumer;

public class MarkReadNotiConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<MarkReadNotiConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<MarkReadNotiEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.markAsReadNotification);

    protected override async Task HandleAsync(MarkReadNotiEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<INotificationReadRepository>();

        await repo.MarkReadAsync(payload.NotificationIds, payload.ReceiverSequenceId, payload.ReadAt, ct);
    }
}