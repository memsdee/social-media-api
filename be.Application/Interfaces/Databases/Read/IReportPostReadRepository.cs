using be.Domain.Documents;

namespace be.Application.Interfaces.Databases.Read;

public interface IReportPostReadRepository
{
    Task AddRangeAsync(List<ReportPostDocument> documents, CancellationToken ct);
}