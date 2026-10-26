using MediatR;
using Microsoft.AspNetCore.Mvc;
using Journal.Application.Auth.Commands;

namespace Journal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(new { Token = result.Token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return Unauthorized(result.Error);
        return Ok(new { Token = result.Token });
    }
}