using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface INotiCmtRepository
{
    Task AddAsync(NotiCmt notiCmt, CancellationToken ct);
}