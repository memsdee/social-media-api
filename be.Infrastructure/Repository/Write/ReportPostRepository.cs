using be.Application.Features.Admin.ReportPost.DeductScoreReportPost;
using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Domain.Enums;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Repository.Write;

public class ReportPostRepository(WriteContext dbContext) : IReportPostRepository
{
    public async Task<int> CountReasonReportPostAsync(List<short> reasonCodes, CancellationToken ct)
    {
        return await dbContext.ReasonReportPost.CountAsync(x => reasonCodes.Contains(x.Code), ct);
    }

    public async Task<Dictionary<short, short>> GetReasonMapAsync(List<short> reasonCodes, CancellationToken ct)
    {
        return await dbContext.ReasonReportPost
            .Where(x => reasonCodes.Contains(x.Code))
            .Select(x => new { x.Code, x.Id })
            .ToDictionaryAsync(x => x.Code, x => x.Id, ct);
    }

    public async Task<short?> GetOtherReasonCodeAsync(CancellationToken ct)
    {
        return await dbContext.ReasonReportPost.OrderByDescending(x => x.Id).Select(x => x.Code)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddUserReportsAsync(List<UserReportPost> reports, CancellationToken ct)
    {
        await dbContext.UserReportPost.AddRangeAsync(reports, ct);
    }

    public async Task<bool> HasAlreadyReportedAsync(short reporterId, short postId, List<short> reasonIds,
        CancellationToken ct)
    {
        return await dbContext.UserReportPost
            .AnyAsync(x => x.ReporterId == reporterId && x.ReportedPost == postId && reasonIds.Contains(x.ReportCode),
                ct);
    }

    public async Task<List<short>> GetReportIdsByPostIdAsync(int postId, CancellationToken ct)
    {
        return await dbContext.UserReportPost.Where(x => x.ReportedPost == postId).Select(x => x.Id).ToListAsync(ct);
    }

    public async Task AddAdminResolveReportLogsAsync(IEnumerable<AdminResolveReportLog> logs, CancellationToken ct)
    {
        await dbContext.AdminResolveReportLogs.AddRangeAsync(logs, ct);
    }

    public async Task UpdateReportStatusAsync(IEnumerable<short> reportIds, StatusReportPostEnum status,
        CancellationToken ct)
    {
        await dbContext.UserReportPost.Where(x => reportIds.Contains(x.Id))
            .ExecuteUpdateAsync(c => c.SetProperty(v => v.Status, status), ct);
    }

    public async Task<ReportPostDetailDto?> GetReportDetailAsync(short reportId, CancellationToken ct)
    {
        return await dbContext.UserReportPost
            .Where(x => x.Id == reportId && x.Status == StatusReportPostEnum.pending)
            .Select(x => new ReportPostDetailDto
            {
                Id = x.Id,
                ReportCode = x.ReportCode,
                PostId = x.ReportedPost,
                PostScore = x.PostNavi.ScoreReport,
                AccountId = x.PostNavi.UserNavi.AccountNavi.Id,
                AccountScore = x.PostNavi.UserNavi.AccountNavi.Score,
                PostIdPublic = x.PostNavi.IdPublic
            })
            .FirstOrDefaultAsync(ct);
    }
}