using be.Application.Interfaces.Services;
using be.Infrastructure.Common.Appsetting;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Services;

public class ScoreService(IOptions<ScoreSettings> scoreSettings) : IScoreService
{
    private readonly ScoreSettings _settings = scoreSettings.Value;

    public int GetSevereAccountScore()
    {
        return _settings.Score.Severe.Account;
    }

    public int GetSeverePostScore()
    {
        return _settings.Score.Severe.Post;
    }

    public int GetDeleteAccountScore()
    {
        return _settings.DeleteAccount;
    }

    public int GetDeletePostScore()
    {
        return _settings.DeletePost;
    }

    public short[] GetLightReasonCodes()
    {
        return _settings.Reason.LightCodes;
    }

    public short[] GetSevereReasonCodes()
    {
        return _settings.Reason.SevereCodes;
    }

    public (int Post, int Account) GetLightScore()
    {
        return (_settings.Score.Light.Post, _settings.Score.Light.Account);
    }

    public (int Post, int Account) GetMediumScore()
    {
        return (_settings.Score.Medium.Post, _settings.Score.Medium.Account);
    }

    public (int Post, int Account) GetSevereScore()
    {
        return (_settings.Score.Severe.Post, _settings.Score.Severe.Account);
    }
}