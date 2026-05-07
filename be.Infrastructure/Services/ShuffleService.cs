using System.Text.Json;
using be.Application.Interfaces.Services;
using be.Domain.Entities;
using be.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace be.Infrastructure.Services;

public class ShuffleService(WriteContext dbContext) : IQuestion
{
    public async Task CreatePlaylistAsync(short userId, CancellationToken cancellationToken)
    {
        var questionId = await dbContext.Questions.Select(x => x.Id).ToListAsync(cancellationToken);
        if (questionId.Count == 0) throw new Exception();

        var playlist = new QuestionPlaylist
        {
            UserId = userId,
            Questions = JsonSerializer.Serialize(questionId.OrderBy(_ => Random.Shared.Next()).ToList()),
            CurrentIndex = 0
        };
        await dbContext.QuestionPlaylists.AddAsync(playlist, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetQuestionAsync(short userId, CancellationToken cancellationToken)
    {
        var metaData = await dbContext.QuestionPlaylists.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
                       ?? throw new Exception();

        var listQuestion = JsonSerializer.Deserialize<List<short>>(metaData.Questions);
        if (listQuestion is null || listQuestion.Count == 0) throw new Exception();

        //nhớ thêm logic kiểm tra nếu hết playlist thì tạo mới

        var questionIndex = listQuestion[metaData.CurrentIndex];

        var log = await dbContext.UserQuestionLogs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ShowAt.Date == DateTime.Today && x.UserId == userId, cancellationToken);

        if (log is null)
        {
            metaData.CurrentIndex++;

            var totalQuestions =
                await dbContext.UserQuestionLogs.CountAsync(x => x.UserId == userId, cancellationToken);


            var newLog = new UserQuestionLog
            {
                UserId = userId,
                QuestionId = questionIndex,
                TotalQuestions = (short)++totalQuestions
            };

            await dbContext.UserQuestionLogs.AddAsync(newLog, cancellationToken);
        }
        else
        {
            questionIndex = log.QuestionId;
        }

        var question = await dbContext.Questions.AsNoTracking()
                           .Where(x => x.Id == questionIndex).Select(x => (string?)x.Content)
                           .FirstOrDefaultAsync(cancellationToken)
                       ?? throw new Exception();

        await dbContext.SaveChangesAsync(cancellationToken);
        return question;
    }
}