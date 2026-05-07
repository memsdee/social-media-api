namespace be.Application.Interfaces.BackgroundJob;

public interface IPostBgJob
{
    Task DeletePostByScore(CancellationToken ctx);
    Task CalculatePostScore(CancellationToken ctx);
}