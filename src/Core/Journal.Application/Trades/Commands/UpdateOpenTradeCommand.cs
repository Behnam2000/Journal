using MediatR;
using Journal.Application.Common.Interfaces;

namespace Journal.Application.Trades.Commands;

public class UpdateOpenTradeCommand : IRequest<Trade>
{
    public long Id { get; set; }
    public string Pair { get; set; }
    public string Exchange { get; set; }
    public string Direction { get; set; }
    public string Setup { get; set; }
    public string TimeFrame { get; set; }
    public string EntryThesis { get; set; }
    public string Context { get; set; }
    public decimal EntryPlanned { get; set; }
    public decimal Sl { get; set; }
    public decimal Tp { get; set; }
    public decimal Size { get; set; }
    public decimal Leverage { get; set; }
    public decimal EntryActual { get; set; }
    public string PsyState { get; set; }
    public decimal RiskPercent { get; set; }
    public decimal RiskAmount { get; set; }
    public decimal RrPlanned { get; set; }
}

public class UpdateOpenTradeCommandHandler : IRequestHandler<UpdateOpenTradeCommand, Trade>
{
    private readonly IAppDbContext _context;

    public UpdateOpenTradeCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Trade> Handle(UpdateOpenTradeCommand request, CancellationToken cancellationToken)
    {
        var trade = await _context.Trades.FindAsync(new object[] { request.Id }, cancellationToken);

        if (trade == null)
        {
            throw new Exception("Trade not found.");
        }

        // Update the editable entry & planning metrics
        trade.Pair = request.Pair;
        trade.Exchange = request.Exchange;
        trade.Direction = request.Direction;
        trade.Setup = request.Setup;
        trade.TimeFrame = request.TimeFrame;
        trade.EntryThesis = request.EntryThesis;
        trade.Context = request.Context;
        trade.EntryPlanned = request.EntryPlanned;
        trade.Sl = request.Sl;
        trade.Tp = request.Tp;
        trade.Size = request.Size;
        trade.Leverage = request.Leverage;
        trade.EntryActual = request.EntryActual;
        trade.PsyState = request.PsyState;
        trade.RiskPercent = request.RiskPercent;
        trade.RiskAmount = request.RiskAmount;
        trade.RrPlanned = request.RrPlanned;

        await _context.SaveChangesAsync(cancellationToken);

        return trade;
    }
}
