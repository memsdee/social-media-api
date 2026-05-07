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

public class PostConsumer(
    IOptions<KafkaSettings> kafkaOptions,
    IProducer<string, string> dltProducer,
    ILogger<PostConsumer> logger,
    IServiceScopeFactory scopeFactory)
    : BaseKafkaConsumer<PostEvent>(kafkaOptions, dltProducer, logger)
{
    protected override string Topic => nameof(OutboxTopicEnum.post);

    protected override async Task HandleAsync(PostEvent payload, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPostReadRepository>();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserReadRepository>();

        if (payload.IsDelete)
        {
            await repo.UpdateStatusAsync(payload.PublicId, payload.Status, ct);
            return;
        }

        var user = await userRepo.GetUser3Async(payload.UserPublicId, ct);
        if (user == null)
        {
            logger.LogWarning("Không tìm thấy user {UserPublicId} để đồng bộ bài viết {PostPublicId}",
                payload.UserPublicId, payload.PublicId);
            return;
        }

        var document = new PostDocument
        {
            SequenceId = payload.SequenceId,
            IdPublic = payload.PublicId,
            UserIdPublic = payload.UserPublicId,
            UserName = user.Name,
            UserAvatar = user.Avatar,
            Content = payload.Content,
            Status = payload.Status,
            CreateAt = payload.CreatedAt,
            TotalCmt = 0,
            TotalLike = 0,
            TotalDislike = 0,
            ScoreTrend = 0,
            Images = payload.Images.Select(i => new PostImageReadModel
            {
                Image = i.Image,
                ImageType = i.ImageType,
                ImageGroupId = i.ImageGroupId
            }).ToList()
        };

        await repo.AddAsync(document, ct);
    }
}