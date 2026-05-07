using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Account.Link.LinkGoogle;

public class LinkGoogleCommand : IRequest<BaseResponse<bool>>
{
    public string Code
    {
        get;
        set => field = value.Trim();
    } = null!;
}