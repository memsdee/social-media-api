using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface IUseridChangeLogsRepository
{
    Task<DateTimeOffset?> GetCreateAtByPublicUserId(short pritaveUserId, CancellationToken cancellationToken);
    Task AddAsync(UseridChangeLog input, CancellationToken cancellationToken);
}