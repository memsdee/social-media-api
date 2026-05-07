using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Account.Link.UnlinkGoogle;

public class UnlinkGoogleCommand : IRequest<BaseResponse<bool>>
{
}