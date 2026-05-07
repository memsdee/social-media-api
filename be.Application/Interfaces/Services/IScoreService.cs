namespace be.Application.Interfaces.Services;

public interface IScoreService
{
    int GetSevereAccountScore();
    int GetSeverePostScore();
    int GetDeleteAccountScore();
    int GetDeletePostScore();
    short[] GetLightReasonCodes();
    short[] GetSevereReasonCodes();
    (int Post, int Account) GetLightScore();
    (int Post, int Account) GetMediumScore();
    (int Post, int Account) GetSevereScore();
}