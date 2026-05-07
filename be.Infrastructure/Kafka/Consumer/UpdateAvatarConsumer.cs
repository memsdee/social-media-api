using be.Application.Dtos.EventBus;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Enums;
using be.Infrastructure.Common.Appsetting;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Kafka.Consumer;

public class UpdateAvatarConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<UpdateAvatarConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<UpdateAvatarEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.updateAvatar);

    protected override async Task HandleAsync(UpdateAvatarEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();

        await repo.UpdateAvatarAsync(payload.PrivateUserId, payload.Avatar, ct);
    }
}