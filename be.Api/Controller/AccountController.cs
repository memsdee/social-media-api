using be.Application.Features.Account.Link.LinkGoogle;
using be.Application.Features.Account.Link.LinkPass;
using be.Application.Features.Account.Link.UnlinkGoogle;
using be.Infrastructure.Common.Appsetting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace be.Api.Controller;

[Route("api/account")]
[ApiController]
public class AccountController(
    IMediator mediator,
    IOptions<JwtSettings> jwtSetiing)
    : ControllerBase
{
    private readonly JwtSettings _jwtSettings = jwtSetiing.Value;

    [EnableRateLimiting("UserStrictPolicy")]
    [HttpPost("link-google")]
    [Authorize("user")]
    public async Task<IActionResult> LinkGoogle(LinkGoogleCommand request, CancellationToken cancellation)
    {
        var x = await mediator.Send(request, cancellation);
        return Ok(x);
    }

    [EnableRateLimiting("UserStrictPolicy")]
    [HttpPost("unlink-google")]
    [Authorize("user")]
    public async Task<IActionResult> UnlinkGoogle(UnlinkGoogleCommand request, CancellationToken cancellation)
    {
        var x = await mediator.Send(request, cancellation);
        return Ok(x);
    }


    [EnableRateLimiting("UserStrictPolicy")]
    [HttpPost("link-pass")]
    [Authorize("user")]
    public async Task<IActionResult> LinkPass(LinkPassCommand request, CancellationToken cancellation)
    {
        var x = await mediator.Send(request, cancellation);
        return Ok(x);
    }
}