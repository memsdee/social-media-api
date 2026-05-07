using be.Domain.Entities;

namespace be.Application.Interfaces.Databases.Write;

public interface IUsernameChangeLogsRepository
{
    Task<DateTimeOffset?> GetCreateAtByPritaveUserId(short pritaveUserId, CancellationToken cancellationToken);
    Task AddAsync(UsernameChangeLog input, CancellationToken cancellationToken);
}