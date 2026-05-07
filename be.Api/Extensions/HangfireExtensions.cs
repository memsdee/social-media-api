using be.Application.Interfaces.BackgroundJob;
using Hangfire;

namespace be.Api.Extensions;

public static class HangfireExtensions
{
    public static void RegisterRecurringJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var jobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        jobs.AddOrUpdate<IPostBgJob>(
            "post-scoring-job",
            service => service.CalculatePostScore(CancellationToken.None),
            Cron.MinuteInterval(5));

        jobs.AddOrUpdate<IPostBgJob>(
            "post-del-by-score-job",
            service => service.DeletePostByScore(CancellationToken.None),
            Cron.Hourly());

        jobs.AddOrUpdate<ITokenBgJob>(
            "token-job",
            service => service.ClearExpiredTokensAsync(CancellationToken.None),
            Cron.Daily(),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh")
            });
    }
}