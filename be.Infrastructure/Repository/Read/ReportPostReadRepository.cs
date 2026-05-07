using be.Application.Interfaces.Databases.Read;
using be.Domain.Documents;
using be.Infrastructure.Database;

namespace be.Infrastructure.Repository.Read;

public class ReportPostReadRepository(ReadContext dbContext) : IReportPostReadRepository
{
    public async Task AddRangeAsync(List<ReportPostDocument> documents, CancellationToken ct)
    {
        if (documents.Count == 0) return;
        await dbContext.Collection<ReportPostDocument>().InsertManyAsync(documents, cancellationToken: ct);
    }
}