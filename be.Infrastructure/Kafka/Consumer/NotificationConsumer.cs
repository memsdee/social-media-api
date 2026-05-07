using be.Application.Dtos.EventBus;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Domain.Enums;
using be.Infrastructure.Common.Appsetting;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Kafka.Consumer;

public class NotificationConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<NotificationConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<NotiFollowEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.notification);

    protected override async Task HandleAsync(NotiFollowEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<INotificationReadRepository>();

        await repo.AddAsync(new NotificationDocument
        {
            SequenceId = payload.SequenceId,
            ReceiveSequenceId = payload.ReciverPrivateUserId,
            SenderSequenceId = payload.SenderPrivateUserId,
            Target = payload.Target,
            Action = payload.Action,
            CreateAt = payload.CreatedAt
        }, ct);
    }
}