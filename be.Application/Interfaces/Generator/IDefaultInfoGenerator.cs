namespace be.Application.Interfaces.Generator;

public interface IDefaultInfoGenerator
{
    Guid Avatar { get; }
    string DeletedName { get; }
    Guid DeletedAvatar { get; }
}