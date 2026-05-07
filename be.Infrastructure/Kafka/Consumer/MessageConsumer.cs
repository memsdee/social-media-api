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

public class MessageConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<MessageConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<MessageSentEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.message);

    protected override async Task HandleAsync(MessageSentEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var messageRepo = scope.ServiceProvider.GetRequiredService<IMessageReadRepository>();
        var conversationRepo = scope.ServiceProvider.GetRequiredService<IConversationReadRepository>();

        var messageDocument = new MessageDocument
        {
            SequenceId = payload.SequenceId,
            ConversationSequeceId = payload.ConversationSequeceId,
            ConversationId = payload.ConversationId,
            SenderSequenceId = payload.SenderSequenceId,
            SenderPublicId = payload.SenderPublicId,
            Content = payload.Content,
            Type = payload.Type,
            CreatedAt = payload.CreatedAt,
            SeenBy = []
        };

        await messageRepo.AddAsync(messageDocument, ct);
        await conversationRepo.UpdateNewMessageAsync(payload, ct);
    }
}