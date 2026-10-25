using Journal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Journal.Application.Trades.Queries;

public class GetAllTradesQuery : IRequest<List<Trade>>
{
}

public class GetAllTradesQueryHandler : IRequestHandler<GetAllTradesQuery, List<Trade>>
{
    private readonly IAppDbContext _context;

    public GetAllTradesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Trade>> Handle(GetAllTradesQuery request, CancellationToken cancellationToken)
    {
        var trades = await _context.Trades
            .OrderByDescending(t => t.EntryTime)
            .ToListAsync(cancellationToken);

        return trades;
    }
}
