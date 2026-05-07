namespace be.Application.Interfaces.Databases.Write;

public interface IFeedbackRepository
{
    Task AddAsync(string content, short userId, DateTimeOffset createdAt, CancellationToken cancellationToken);
}