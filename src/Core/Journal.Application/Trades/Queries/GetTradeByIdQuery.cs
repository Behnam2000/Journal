using Journal.Application.Common.Interfaces;
using MediatR;
using Journal.Application.Common.Interfaces;

namespace Journal.Application.Trades.Queries;

public class GetTradeByIdQuery : IRequest<Trade?>
{
    public long Id { get; set; }
}

public class GetTradeByIdQueryHandler : IRequestHandler<GetTradeByIdQuery, Trade?>
{
    private readonly IAppDbContext _context;

    public GetTradeByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Trade?> Handle(GetTradeByIdQuery request, CancellationToken cancellationToken)
    {
        var trade = await _context.Trades.FindAsync(new object[] { request.Id }, cancellationToken);

        return trade;
    }
}
