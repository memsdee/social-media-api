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

public class ReportPostConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<ReportPostConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<ReportPostEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.reportPost);

    protected override async Task HandleAsync(ReportPostEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IReportPostReadRepository>();

        var documents = payload.Reports.Select(x => new ReportPostDocument
        {
            SequenceId = x.Id,
            ReporterPublicId = x.ReporterPublicId,
            ReporterName = x.ReporterName,
            ReporterAvatar = x.ReporterAvatar,
            ReporterMail = x.ReporterMail,
            PostPublicId = x.PostPublicId,
            PostSequenceId = x.PostSequenceId,
            ReasonCode = x.ReasonCode,
            OtherReason = x.OtherReason,
            Status = x.Status,
            CreatedAt = x.CreatedAt
        }).ToList();

        await repo.AddRangeAsync(documents, ct);
    }
}