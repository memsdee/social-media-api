using be.Application.Interfaces.Generator;
using be.Infrastructure.Common.Appsetting;
using Microsoft.Extensions.Options;

namespace be.Infrastructure.Services.Generator;

public class DefaultInfoGenerator(IOptions<DefaultInfoSettings> defaultInfo) : IDefaultInfoGenerator
{
    public Guid Avatar => defaultInfo.Value.Avatar;
    public Guid DeletedAvatar => defaultInfo.Value.DeletedAvatar;
    public string DeletedName => defaultInfo.Value.DeletedName;
}