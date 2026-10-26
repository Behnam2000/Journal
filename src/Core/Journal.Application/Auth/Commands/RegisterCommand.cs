using Journal.Application.Common.Interfaces;
using MediatR;

namespace Journal.Application.Auth.Commands;

public class RegisterCommand : IRequest<(bool Success, string? Token, string? Error)>
{
    public string Email { get; set; }
    public string Password { get; set; }

}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, (bool Success, string? Token, string? Error)>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(bool Success, string? Token, string? Error)> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var response = await _identityService.RegisterUserAsync(request.Email, request.Password);

        return (response.Result, response.Token, response.Error);
    }

}
