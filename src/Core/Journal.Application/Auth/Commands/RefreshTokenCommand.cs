using System.Security.Principal;
using Journal.Application.Common.Interfaces;
using MediatR;

namespace Journal.Application.Auth.Commands;

public class RefreshTokenCommand : IRequest<(bool Success, string? Token, string? RefreshToken, string? Error)>
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, (bool Success, string? Token, string? RefreshToken, string? Error)>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(bool Success, string? Token, string? RefreshToken, string? Error)> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var response = await _identityService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
        return (response.Result, response.Token, response.RefreshToken, response.Error);
    }
}