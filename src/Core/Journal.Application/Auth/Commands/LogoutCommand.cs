using Journal.Application.Common.Interfaces;
using MediatR;

namespace Journal.Application.Auth.Commands;

public class LogoutCommand : IRequest<bool>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IIdentityService _identityService;

    public LogoutCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LogoutAsync(request.RefreshToken);
    }
}