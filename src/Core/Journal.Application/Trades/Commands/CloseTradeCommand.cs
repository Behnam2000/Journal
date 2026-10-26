using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Journal.Application.Trades.Commands;

public class CloseTradeCommand : IRequest<Trade>
{
    public long Id { get; set; }
    public decimal Exit { get; set; }
    public DateTimeOffset ExitTime { get; set; }
    public string ExitReason { get; set; }
    public decimal Fees { get; set; }
    public decimal PlAmount { get; set; }
    public decimal PlPercent { get; set; }
    public decimal RealizedR { get; set; }
    public string Outcome { get; set; }
    public bool FollowedRules { get; set; }
}

public class CloseTradeCommandHandler : IRequestHandler<CloseTradeCommand, Trade>
{
    private readonly IAppDbContext _context;

    public CloseTradeCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Trade> Handle(CloseTradeCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the active trade and include its parent journal
        var trade = await _context.Trades
            .Include(t => t.DailyJournal)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (trade == null)
        {
            throw new Exception("Trade Not Found");
        }

        // 2. Update the exit fields
        trade.Exit = request.Exit;
        trade.ExitTime = request.ExitTime;
        trade.ExitReason = request.ExitReason;
        trade.Fees = request.Fees;
        trade.FollowedRules = request.FollowedRules;
        trade.PlAmount = request.PlAmount;
        trade.PlPercent = request.PlPercent;
        trade.RealizedR = request.RealizedR;
        trade.Outcome = request.Outcome;
        trade.Fees = request.Fees;

        // ! subtract the fee directly from the submitted P&L for that specific trade
        trade.PlAmount = request.PlAmount - request.Fees;

        // 3. Update the daily journal Win/Loss counters based on the outcome
        if (request.Outcome.Equals("Win", StringComparison.OrdinalIgnoreCase))
        {
            trade.DailyJournal.Wins += 1;
        }
        // else if (request.Outcome.Equals("Loss", StringComparison.OrdinalIgnoreCase))
        // {
        //     trade.DailyJournal.Losses += 1;
        // }

        await _context.SaveChangesAsync(cancellationToken);

        return trade;
    }
}
