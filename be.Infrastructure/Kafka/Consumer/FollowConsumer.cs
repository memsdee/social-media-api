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

public class FollowConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<FollowConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<FollowEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.follow);

    protected override async Task HandleAsync(FollowEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IFollowReadRepository>();

        await repo.AddAsync(new FollowDocument
        {
            FollowerSequenceId = payload.FollowerSequenceId,
            FollowerIdPublic = payload.FollowerIdPublic,
            FollowerName = payload.FollowerName,
            FollowerAvatar = payload.FollowerAvatar,
            FollowerIsDeleteAccount = payload.FollowerIsDeleteAccount,
            FolloweeSequenceId = payload.FolloweeSequenceId,
            FolloweeIdPublic = payload.FolloweeIdPublic,
            FolloweeName = payload.FolloweeName,
            FolloweeAvatar = payload.FolloweeAvatar,
            FolloweeIsDeleteAccount = payload.FolloweeIsDeleteAccount,
            CreatedAt = payload.CreateAt
        }, ct);
    }
}