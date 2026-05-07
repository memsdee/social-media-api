namespace be.Application.Interfaces.BackgroundJob;

public interface ITokenBgJob
{
    Task ClearExpiredTokensAsync(CancellationToken ctx);
}