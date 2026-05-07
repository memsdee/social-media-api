using be.Application.Dtos.Notification;
using be.Application.Dtos.Pagination;

namespace be.Application.Features.Notification.GetListNoti;

public class GetListNotiResponse
{
    public List<NotificationDto> Notifications { get; set; } = [];
    public PagiResult PageProfile { get; set; } = null!;
}