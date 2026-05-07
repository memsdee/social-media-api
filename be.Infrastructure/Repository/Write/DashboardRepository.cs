using be.Application.Common.Constants;
using be.Application.Dtos.Pagination;
using be.Application.Features.Dashboard.Account.GetListAccount;
using be.Application.Features.Dashboard.Account.SummaryAccount;
using be.Application.Features.Dashboard.Analytics.Chart;
using be.Application.Features.Dashboard.Analytics.Summary;
using be.Application.Features.Dashboard.Post.GetListPost;
using be.Application.Features.Dashboard.Post.Summary;
using be.Application.Features.Dashboard.ReportPost.GetListReportPost;
using be.Application.Features.Dashboard.ReportPost.Summary;
using be.Application.Interfaces.Databases.Write;
using be.Application.Interfaces.Services;
using be.Domain.Enums;
using be.Infrastructure.Database;
using be.Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class DashboardRepository(WriteContext dbContext, IFormat format) : IDashboardRepository
{
    public async Task<(List<AccountDto> Items, OffsetPageInfo PageInfo)> GetPagedAccountsAsync(int page, int pageSize,
        CancellationToken ct)
    {
        var query = dbContext.Users
            .IgnoreQueryFilters()
            .Select(x => new AccountDto
            {
                Id = x.AccountId,
                UserName = x.Name,
                UserId = x.UserId,
                Mail = x.AccountNavi.Mail,
                Avatar = format.FormatImageUrl(x.Avatar, x.UserId),
                Score = x.AccountNavi.Score,
                Status = x.AccountNavi.Status,
                CreatedAt = x.AccountNavi.CreatAt
            });

        var total = await query.CountAsync(ct);
        var items = await WriteOffsetPagiHelper.Apply(query, page, pageSize, x => x.CreatedAt)
            .ToListAsync(ct);

        return (items, WriteOffsetPagiHelper.Calculate(total, page, pageSize));
    }

    public async Task<(List<PostDashboardDto> Items, OffsetPageInfo PageInfo)> GetPagedPostsAsync(int page,
        int pageSize, CancellationToken ct)
    {
        var query = dbContext.Posts
            .IgnoreQueryFilters()
            .Select(x => new PostDashboardDto
            {
                Id = x.Id,
                Content = x.Content,
                UserName = x.UserNavi.Name,
                UserId = x.UserNavi.UserId,
                UserMail = x.UserNavi.AccountNavi.Mail,
                UserAvatar = format.FormatImageUrl(x.UserNavi.Avatar, x.UserNavi.UserId),
                TotalComment = x.TotalComment,
                ScoreTrend = x.ScoreTrend,
                ScoreReport = x.ScoreReport,
                Status = x.Status,
                CreatedAt = x.CreatAt
            });

        var total = await query.CountAsync(ct);
        var items = await WriteOffsetPagiHelper.Apply(query, page, pageSize, x => x.CreatedAt)
            .ToListAsync(ct);

        return (items, WriteOffsetPagiHelper.Calculate(total, page, pageSize));
    }

    public async Task<(List<ReportPostDto> Items, OffsetPageInfo PageInfo)> GetPagedReportsAsync(int page, int pageSize,
        CancellationToken ct)
    {
        var query = dbContext.UserReportPost
            .Select(x => new ReportPostDto
            {
                UserMail = x.UserNavi.AccountNavi.Mail,
                UserName = x.UserNavi.Name,
                UserAvatar = format.FormatImageUrl(x.UserNavi.Avatar, x.UserNavi.UserId),
                PostId = x.ReportedPost,
                Reason = x.ReasonReportPostNavi.Code,
                OtherReason = x.OtherReason,
                Date = x.CreateAt,
                IdRow = x.Id,
                Status = x.Status
            });

        var total = await query.CountAsync(ct);
        var items = await WriteOffsetPagiHelper.Apply(query, page, pageSize, x => x.Date)
            .ToListAsync(ct);

        return (items, WriteOffsetPagiHelper.Calculate(total, page, pageSize));
    }

    public async Task<SummaryAccountResponse> GetAccountSummaryAsync(CancellationToken ct)
    {
        return await dbContext.Accounts
            .IgnoreQueryFilters()
            .Select(x => new
            {
                IsActive = x.Status == StatusAccountEnum.active,
                IsBanned = x.Status == StatusAccountEnum.banned,
                isToday = x.CreatAt.Date == DateTimeOffset.UtcNow.Date
            }).GroupBy(_ => 1)
            .Select(g => new SummaryAccountResponse
            {
                Total = (short)g.Count(),
                TotalActive = (short)g.Count(x => x.IsActive),
                TotallBanned = (short)g.Count(x => x.IsBanned),
                TotalToday = (short)g.Count(x => x.isToday)
            }).FirstOrDefaultAsync(ct) ?? new SummaryAccountResponse
        {
            Total = 0,
            TotalActive = 0,
            TotallBanned = 0,
            TotalToday = 0
        };
    }

    public async Task<SummaryPostResponse> GetPostSummaryAsync(CancellationToken ct)
    {
        return await dbContext.Posts.IgnoreQueryFilters()
            .Select(x => new
            {
                Today = x.CreatAt.Date == DateTimeOffset.UtcNow.Date,
                Month = x.CreatAt.Month == DateTimeOffset.UtcNow.Month && x.CreatAt.Year == DateTimeOffset.UtcNow.Year,
                Delected = x.Status == StatusPostEnum.deleted
            })
            .GroupBy(_ => 1)
            .Select(g => new SummaryPostResponse
            {
                TotalPost = g.Count(),
                TotalToday = g.Count(x => x.Today),
                TotalMonth = g.Count(x => x.Month),
                TotalDeleted = g.Count(x => x.Delected)
            }).FirstOrDefaultAsync(ct) ?? new SummaryPostResponse
        {
            TotalPost = 0,
            TotalToday = 0,
            TotalMonth = 0,
            TotalDeleted = 0
        };
    }

    public async Task<SummaryReportPostResponse> GetReportSummaryAsync(CancellationToken ct)
    {
        var today = DateTimeOffset.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) diff += 7;
        var startOfThisWeek = today.AddDays(-diff).Date;
        var startOfLastWeek = startOfThisWeek.AddDays(-7);

        var stats = await dbContext.UserReportPost
            .IgnoreQueryFilters()
            .Select(x => new
            {
                IsToday = x.CreateAt.Date == today,
                IsYesterday = x.CreateAt.Date == yesterday,
                IsThisWeek = x.CreateAt.Date >= startOfThisWeek,
                IsLastWeek = x.CreateAt.Date >= startOfLastWeek && x.CreateAt.Date < startOfThisWeek,
                IsResolved = x.Status == StatusReportPostEnum.resolved || x.Status == StatusReportPostEnum.rejected
            })
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalToday = (short)g.Count(x => x.IsToday),
                TotalYesterday = (short)g.Count(x => x.IsYesterday),
                TotalThisWeek = (short)g.Count(x => x.IsThisWeek),
                TotalLastWeek = (short)g.Count(x => x.IsLastWeek),
                Total = (short)g.Count(),
                TotalResolved = (short)g.Count(x => x.IsResolved)
            })
            .FirstOrDefaultAsync(ct) ?? new
        {
            TotalToday = (short)0,
            TotalYesterday = (short)0,
            TotalThisWeek = (short)0,
            TotalLastWeek = (short)0,
            Total = (short)0,
            TotalResolved = (short)0
        };

        return new SummaryReportPostResponse
        {
            Total = stats.Total,
            TotalToday = stats.TotalToday,
            DiffYesterday = (short)(stats.TotalToday - stats.TotalYesterday),
            TotalThisWeek = stats.TotalThisWeek,
            DiffLastWeek = (short)(stats.TotalThisWeek - stats.TotalLastWeek),
            TotalResolved = stats.TotalResolved
        };
    }

    public async Task<SummaxryResponse> GetAnalyticsSummaryAsync(CancellationToken ct)
    {
        var today = DateTimeOffset.UtcNow.Date;

        var accountStats = await dbContext.Accounts
            .IgnoreQueryFilters()
            .Where(x => x.Role == RoleEnum.user)
            .GroupBy(_ => 1)
            .Select(g => new Account
            {
                Total = g.Count(),
                Today = g.Count(x => x.CreatAt.Date == today)
            })
            .FirstOrDefaultAsync(ct) ?? new Account { Total = 0, Today = 0 };

        var postStats = await dbContext.Posts
            .IgnoreQueryFilters()
            .GroupBy(_ => 1)
            .Select(g => new Post
            {
                Total = g.Count(),
                Today = g.Count(x => x.CreatAt.Date == today)
            })
            .FirstOrDefaultAsync(ct) ?? new Post { Total = 0, Today = 0 };

        var commentStats = await dbContext.Comments
            .IgnoreQueryFilters()
            .GroupBy(_ => 1)
            .Select(g => new Comment
            {
                Total = g.Count(),
                Today = g.Count(x => x.CreatAt.Date == today)
            })
            .FirstOrDefaultAsync(ct) ?? new Comment { Total = 0, Today = 0 };

        var reportStats = await dbContext.UserReportPost
            .IgnoreQueryFilters()
            .GroupBy(_ => 1)
            .Select(g => new Report
            {
                Total = g.Count(),
                Today = g.Count(x => x.CreateAt.Date == today)
            })
            .FirstOrDefaultAsync(ct) ?? new Report { Total = 0, Today = 0 };

        return new SummaxryResponse
        {
            Account = accountStats,
            Posts = postStats,
            Comments = commentStats,
            Reports = reportStats
        };
    }

    public async Task<ChartItemResponse> GetAccountChartAsync(string period, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var data = new List<(string Label, int Count)>();

        if (period == ChartPeriod.Week)
        {
            var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startDay = now.AddDays(-diff).Date;
            var startDate = new DateTimeOffset(startDay.Year, startDay.Month, startDay.Day, 0, 0, 0, TimeSpan.Zero);

            var dbData = await dbContext.Accounts
                .IgnoreQueryFilters()
                .Where(x => x.CreatAt >= startDate && x.CreatAt <= now)
                .GroupBy(x => x.CreatAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            for (var i = 0; i < 7; i++)
            {
                var date = startDay.AddDays(i);
                var item = dbData.FirstOrDefault(x => x.Date == date);
                data.Add((date.DayOfWeek.ToString(), item?.Count ?? 0));
            }
        }
        else if (period == ChartPeriod.Month)
        {
            var startDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var dbData = await dbContext.Accounts
                .IgnoreQueryFilters()
                .Where(x => x.CreatAt >= startDate && x.CreatAt <= now)
                .GroupBy(x => x.CreatAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            for (var i = 1; i <= now.Day; i++)
            {
                var date = new DateTime(now.Year, now.Month, i);
                var item = dbData.FirstOrDefault(x => x.Date == date);
                data.Add((i.ToString(), item?.Count ?? 0));
            }
        }
        else if (period == ChartPeriod.Year)
        {
            var startDate = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var dbData = await dbContext.Accounts
                .IgnoreQueryFilters()
                .Where(x => x.CreatAt >= startDate && x.CreatAt <= now)
                .GroupBy(x => x.CreatAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            for (var i = 1; i <= now.Month; i++)
            {
                var item = dbData.FirstOrDefault(x => x.Month == i);
                data.Add((i.ToString(), item?.Count ?? 0));
            }
        }

        return new ChartItemResponse
        {
            Labels = data.Select(x => x.Label).ToList(),
            Data = data.Select(x => x.Count).ToList()
        };
    }

    public async Task<ChartItemResponse> GetPostChartAsync(string period, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var data = new List<(string Label, int Count)>();

        if (period == ChartPeriod.Week)
        {
            var diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startDay = now.AddDays(-diff).Date;
            var startDate = new DateTimeOffset(startDay.Year, startDay.Month, startDay.Day, 0, 0, 0, TimeSpan.Zero);

            var dbData = await dbContext.Posts
                .IgnoreQueryFilters()
                .Where(x => x.CreatAt >= startDate && x.CreatAt <= now)
                .GroupBy(x => x.CreatAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            for (var i = 0; i < 7; i++)
            {
                var date = startDay.AddDays(i);
                var item = dbData.FirstOrDefault(x => x.Date == date);
                data.Add((date.DayOfWeek.ToString(), item?.Count ?? 0));
            }
        }
        else if (period == ChartPeriod.Month)
        {
            var startDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var dbData = await dbContext.Posts
                .IgnoreQueryFilters()
                .Where(x => x.CreatAt >= startDate && x.CreatAt <= now)
                .GroupBy(x => x.CreatAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            for (var i = 1; i <= now.Day; i++)
            {
                var date = new DateTime(now.Year, now.Month, i);
                var item = dbData.FirstOrDefault(x => x.Date == date);
                data.Add((i.ToString(), item?.Count ?? 0));
            }
        }
        else if (period == ChartPeriod.Year)
        {
            var startDate = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var dbData = await dbContext.Posts
                .IgnoreQueryFilters()
                .Where(x => x.CreatAt >= startDate && x.CreatAt <= now)
                .GroupBy(x => x.CreatAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            for (var i = 1; i <= now.Month; i++)
            {
                var item = dbData.FirstOrDefault(x => x.Month == i);
                data.Add((i.ToString(), item?.Count ?? 0));
            }
        }

        return new ChartItemResponse
        {
            Labels = data.Select(x => x.Label).ToList(),
            Data = data.Select(x => x.Count).ToList()
        };
    }
}