using be.Application.Features.Search.SearchAll;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be.Api.Controller;

[Route("api/search")]
[ApiController]
public class SearchController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> SearchAll([FromQuery] SearchAllQuery request, CancellationToken cancellationToken)
    {
        var x = await mediator.Send(request, cancellationToken);
        return Ok(x);
    }
}