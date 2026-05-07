using be.Application.Features.Admin.ReportPost.DeductScoreReportPost;
using be.Domain.Entities;
using be.Domain.Enums;

namespace be.Application.Interfaces.Databases.Write;

public interface IReportPostRepository
{
    Task<int> CountReasonReportPostAsync(List<short> reasonCodes, CancellationToken ct);
    Task<Dictionary<short, short>> GetReasonMapAsync(List<short> reasonCodes, CancellationToken ct);
    Task<short?> GetOtherReasonCodeAsync(CancellationToken ct);
    Task AddUserReportsAsync(List<UserReportPost> reports, CancellationToken ct);
    Task<bool> HasAlreadyReportedAsync(short reporterId, short postId, List<short> reasonIds, CancellationToken ct);
    Task<List<short>> GetReportIdsByPostIdAsync(int postId, CancellationToken ct);
    Task AddAdminResolveReportLogsAsync(IEnumerable<AdminResolveReportLog> logs, CancellationToken ct);
    Task UpdateReportStatusAsync(IEnumerable<short> reportIds, StatusReportPostEnum status, CancellationToken ct);
    Task<ReportPostDetailDto?> GetReportDetailAsync(short reportId, CancellationToken ct);
}