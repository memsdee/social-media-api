namespace be.Application.Interfaces.Services;

public interface IQuestion
{
    Task CreatePlaylistAsync(short userId, CancellationToken cancellationToken);
    Task<string> GetQuestionAsync(short userId, CancellationToken cancellationToken);
}