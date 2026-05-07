using be.Application.Dtos.EventBus;
using be.Application.Interfaces.Databases.Read;
using be.Domain.Enums;
using be.Infrastructure.Common.Appsetting;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Kafka.Consumer;

public class DelAccountConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    IServiceScopeFactory scopeFactory,
    ILogger<DelAccountEvent> logger)
    : BaseKafkaConsumer<DelAccountEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.delAccount);

    protected override async Task HandleAsync(DelAccountEvent payload, CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var postReadRepo = scope.ServiceProvider.GetRequiredService<IPostReadRepository>();
        var commentReadRepo = scope.ServiceProvider.GetRequiredService<ICommentReadRepository>();
        var userReadRepo = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();

        await postReadRepo.MarkAuthorDeletedAsync(payload.PrivateAccountId, stoppingToken);
        await commentReadRepo.MarkAuthorDeletedAsync(payload.PrivateAccountId, stoppingToken);
        await userReadRepo.MarkDeletedAsync(payload.PrivateAccountId, stoppingToken);
    }
}