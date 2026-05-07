using be.Application.Features.Post.Comment.CreateComment;
using be.Application.Features.Post.Comment.DeleteComment;
using be.Application.Features.Post.Comment.GetCommentById;
using be.Application.Features.Post.Comment.GetListComment;
using be.Application.Features.Post.Post.AddPost;
using be.Application.Features.Post.Post.DeletePost;
using be.Application.Features.Post.Post.GetListPost;
using be.Application.Features.Post.Post.GetPost;
using be.Application.Features.Post.Post.ReportPost;
using be.Application.Features.Post.React.DislikePost;
using be.Application.Features.Post.React.LikePost;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace be.Api.Controller;

[Route("api/post")]
[ApiController]
public class PostController(IMediator mediator) : ControllerBase
{
    [EnableRateLimiting("UserGeneralPolicy")]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetListPost([FromQuery] GetListPostQuery request, CancellationToken cancellation)
    {
        var result = await mediator.Send(request, cancellation);
        return Ok(result);
    }

    [EnableRateLimiting("UserStrictPolicy")]
    [HttpPost]
    [Authorize("user")]
    public async Task<IActionResult> AddPost([FromBody] AddPostCommand request, CancellationToken cancellation)
    {
        var x = await mediator.Send(request, cancellation);
        return Ok(x);
    }

}