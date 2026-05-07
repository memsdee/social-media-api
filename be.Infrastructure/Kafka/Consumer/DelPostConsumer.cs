using be.Application.Dtos.EventBus;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Enums;
using be.Infrastructure.Common.Appsetting;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Kafka.Consumer;

public class DelPostConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<DelPostConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<PostDeletedByScoreEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.postDelByScore);

    protected override async Task HandleAsync(PostDeletedByScoreEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPostReadRepository>();

        foreach (var post in payload.ListIdPublicPost)
            await repo.UpdateStatusAsync(post, payload.Status, ct);
    }
}