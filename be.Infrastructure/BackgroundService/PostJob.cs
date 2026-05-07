using be.Application.Dtos.EventBus;
using be.Application.Interfaces.BackgroundJob;
using be.Application.Interfaces.Databases.Write;
using be.Domain.Enums;
using be.Infrastructure.Common.Appsetting;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.BackgroundService;

public class PostJob(
    WriteContext writeContext,
    IOptions<ScoreSettings> scoreOptions,
    IOutboxRepository outboxRepository) : IPostBgJob
{
    public async Task DeletePostByScore(CancellationToken ctx)
    {
        var postIds = await writeContext.Posts
            .IgnoreQueryFilters()
            .Where(x => x.Status == StatusPostEnum.active && x.ScoreReport >= scoreOptions.Value.DeletePost)
            .Select(x => new { x.Id, x.IdPublic })
            .ToListAsync(ctx);

        if (postIds.Count == 0) return;

        await using var transaction = await writeContext.Database.BeginTransactionAsync(ctx);
        try
        {
            await writeContext.Posts
                .IgnoreQueryFilters()
                .Where(x => postIds.Select(arg => arg.Id).Contains(x.Id))
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.Status, StatusPostEnum.deleted), ctx);

            await outboxRepository.AddAsync(
                OutboxTopicEnum.postDelByScore, new PostDeletedByScoreEvent
                {
                    ListIdPublicPost = postIds.Select(x => x.IdPublic).ToList(),
                    Status = StatusPostEnum.deleted
                }
                , ctx);

            await writeContext.SaveChangesAsync(ctx);
            await transaction.CommitAsync(ctx);
        }
        catch
        {
            await transaction.RollbackAsync(ctx);
            throw;
        }
    }

    public async Task CalculatePostScore(CancellationToken ctx)
    {
        await writeContext.Database.ExecuteSqlRawAsync(@"
                        UPDATE ""content"".""posts""
                        SET ""score"" = (""total_like"" + ""total_dislike"" + ""total_comment"" * 3) / 
                                       POWER(EXTRACT(EPOCH FROM (NOW() AT TIME ZONE 'UTC' - ""creat_at"")) / 3600 + 2, 1.2)
                    ", ctx);
    }
}