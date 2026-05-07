using be.Application.Dtos.EventBus;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Enums;
using be.Infrastructure.Common.Appsetting;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Kafka.Consumer;

public class UnfollowConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<UnfollowConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<UnfollowEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.unFollow);

    protected override async Task HandleAsync(UnfollowEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IFollowReadRepository>();

        await repo.UnfollowAsync(payload.FollowerSequenceId, payload.FolloweeSequenceId, ct);
    }
}