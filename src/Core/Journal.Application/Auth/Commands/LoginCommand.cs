using Journal.Application.Common.Interfaces;
using MediatR;

namespace Journal.Application.Auth.Commands;

public class LoginCommand : IRequest<(bool Success, string? Token, string? RefreshToken, string? Error)>
{
    public string Email { get; set; }
    public string Password { get; set; }

}

public class LoginCommandHandler : IRequestHandler<LoginCommand, (bool Success, string? Token, string? RefreshToken, string? Error)>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(bool Success, string? Token, string? RefreshToken, string? Error)> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var response = await _identityService.LoginUserAsync(request.Email, request.Password);

        return (response.Result, response.Token, response.RefreshToken, response.Error);
    }

}
