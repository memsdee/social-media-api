using be.Application.Features.Notification.GetListNoti;
using be.Application.Features.Notification.GetUnreadCount;
using be.Application.Features.Notification.MarkRead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace be.Api.Controller;

[Route("api/notification")]
[ApiController]
public class NotificationController(IMediator mediator) : ControllerBase
{
    [EnableRateLimiting("UserGeneralPolicy")]
    [HttpGet]
    [Authorize("user")]
    public async Task<IActionResult> GetListNoti([FromQuery] GetListNotiQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [EnableRateLimiting("UserGeneralPolicy")]
    [HttpGet("unread-count")]
    [Authorize("user")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUnreadCountQuery(), cancellationToken);
        return Ok(result);
    }

    [EnableRateLimiting("UserGeneralPolicy")]
    [HttpPatch("mark-read")]
    [Authorize("user")]
    public async Task<IActionResult> MarkRead([FromBody] MarkReadCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}