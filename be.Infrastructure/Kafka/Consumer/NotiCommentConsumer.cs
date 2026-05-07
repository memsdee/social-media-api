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

public class NotiCommentConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<NotiCommentConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<NotiCommentEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.notiComment);

    protected override async Task HandleAsync(NotiCommentEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<INotificationReadRepository>();

        await repo.AddAsync(new NotificationDocument
        {
            SequenceId = payload.SequenceId,
            ReceiveSequenceId = payload.ReceiveSequenceId,
            SenderSequenceId = payload.SenderSequenceId,
            Target = payload.Target,
            Action = payload.Action,
            PostSequenceId = payload.PostSequenceId,
            PostPublicId = payload.PostPublicId,
            CmtSequenceId = payload.CmtSequenceId,
            CmtPublicId = payload.CmtPublicId,
            ThumbnailNoti = payload.ThumbnailNoti,
            PreviewContent = payload.PreviewContent,
            CreateAt = payload.CreatedAt
        }, ct);
    }
}