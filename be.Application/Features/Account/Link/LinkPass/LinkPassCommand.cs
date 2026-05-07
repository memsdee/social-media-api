using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Account.Link.LinkPass;

public class LinkPassCommand : IRequest<BaseResponse>
{
    public string Otp
    {
        get;
        set => field = value.Trim();
    } = null!;

    public string Pass { get; set; } = null!;

    public string Mail
    {
        get;
        set => field = value.Trim();
    } = null!;
}