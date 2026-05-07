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

public class ConversationConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<ConversationConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<ConversationCreatedEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.conversation);

    protected override async Task HandleAsync(ConversationCreatedEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IConversationReadRepository>();

        await repo.AddAsync(new ConversationDocument
        {
            SequenceId = payload.SequenceId,
            IdPublic = payload.ConversationPublicId,
            PreviewLastMess = payload.LastMessage,
            LastMessageDate = payload.LastMessageDate,
            LastUpdate = payload.LastUpdate,
            CreatorSequenceId = payload.CreatorSequenceId,
            Type = TypeConversationEnum.single,
            CreateAt = payload.CreateAt,
            KeyPart = payload.KeyPart,
            Participants = payload.Participants
                .Select(x => new Participants
                {
                    UserSequenceId = x.UserSequenceId,
                    UserAvatar = x.UserAvatar ?? Guid.Empty,
                    UserName = x.UserName,
                    UnreadCount = x.UnreadCount
                })
                .ToArray()
        }, ct);
    }
}