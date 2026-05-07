namespace be.Application.Interfaces.Generator;

public interface IJwtTokenGenerator
{
    string Key { get; }
    string Issuer { get; }
    string Audience { get; }
    int ExpHours { get; }
    int RefreshExpDays { get; }
}