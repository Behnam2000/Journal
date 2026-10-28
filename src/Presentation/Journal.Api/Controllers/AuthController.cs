using MediatR;
using Microsoft.AspNetCore.Mvc;
using Journal.Application.Auth.Commands;
using Journal.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

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
        return Ok(new { Token = result.Token, RefreshToken = result.RefreshToken });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return Unauthorized(result.Error);
        return Ok(new { Token = result.Token, RefreshToken = result.RefreshToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success) return Unauthorized(result.Error);

        return Ok(new { Token = result.Token, RefreshToken = result.RefreshToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        var success = await _mediator.Send(command);

        if (!success)
            return BadRequest("Invalid or already revoked token");

        return NoContent(); // 204 No Content
    }
}