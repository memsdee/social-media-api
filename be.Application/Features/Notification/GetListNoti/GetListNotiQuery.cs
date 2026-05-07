using be.Application.Dtos.Shared;
using MediatR;

namespace be.Application.Features.Notification.GetListNoti;

public class GetListNotiQuery : IRequest<BaseResponse<GetListNotiResponse>>
{
    public int Limit { get; set; }
    public string? Cursor { get; set; }
}