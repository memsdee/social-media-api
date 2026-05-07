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

public class CommentConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<CommentConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<CommentCreatedEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.comment);

    protected override async Task HandleAsync(CommentCreatedEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICommentReadRepository>();

        await repo.AddAsync(new CommentDocument
        {
            SequenceId = payload.SequenceId,
            IdPublic = payload.IdPublic,
            PostSquenceId = payload.PostSequenceId,
            PostPublicId = payload.PostPublicId,
            UserSquenceId = payload.UserSequenceId,
            UserIdPublic = payload.UserIdPublic,
            UserName = payload.UserName,
            UserAvatar = payload.UserAvatar,
            Content = payload.Content,
            CreateAt = payload.CreatedAt,
            IsDeleteAccount = false
        }, ct);
    }
}