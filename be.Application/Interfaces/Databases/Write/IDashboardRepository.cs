using be.Application.Dtos.Pagination;
using be.Application.Features.Dashboard.Account.GetListAccount;
using be.Application.Features.Dashboard.Account.SummaryAccount;
using be.Application.Features.Dashboard.Analytics.Chart;
using be.Application.Features.Dashboard.Analytics.Summary;
using be.Application.Features.Dashboard.Post.GetListPost;
using be.Application.Features.Dashboard.Post.Summary;
using be.Application.Features.Dashboard.ReportPost.GetListReportPost;
using be.Application.Features.Dashboard.ReportPost.Summary;

namespace be.Application.Interfaces.Databases.Write;

public interface IDashboardRepository
{
    Task<(List<AccountDto> Items, OffsetPageInfo PageInfo)> GetPagedAccountsAsync(int page, int pageSize,
        CancellationToken ct);

    Task<(List<PostDashboardDto> Items, OffsetPageInfo PageInfo)> GetPagedPostsAsync(int page, int pageSize,
        CancellationToken ct);

    Task<(List<ReportPostDto> Items, OffsetPageInfo PageInfo)> GetPagedReportsAsync(int page, int pageSize,
        CancellationToken ct);

    Task<SummaryAccountResponse> GetAccountSummaryAsync(CancellationToken ct);
    Task<SummaryPostResponse> GetPostSummaryAsync(CancellationToken ct);
    Task<SummaryReportPostResponse> GetReportSummaryAsync(CancellationToken ct);
    Task<SummaxryResponse> GetAnalyticsSummaryAsync(CancellationToken ct);
    Task<ChartItemResponse> GetAccountChartAsync(string period, CancellationToken ct);
    Task<ChartItemResponse> GetPostChartAsync(string period, CancellationToken ct);
}