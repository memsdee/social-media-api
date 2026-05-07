using be.Application.Interfaces.Databases.Write;
using be.Domain.Entities;
using be.Infrastructure.Database;

namespace be.Infrastructure.Repository.Write;

public class FeedbackRepository(WriteContext writeContext) : IFeedbackRepository
{
    public async Task AddAsync(string content, short userId, DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        await writeContext.Feedbacks.AddAsync(new Feedback
        {
            UserId = userId,
            Content = content,
            CreatedAt = createdAt
        }, cancellationToken);
    }
}