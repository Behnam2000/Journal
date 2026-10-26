using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Journal.Application.Trades.Commands;

public class LogTradeCommand : IRequest<Trade>
{
    public long DailyJournalId { get; set; }
    public int TradeNumber { get; set; }
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
    public DateTimeOffset EntryTime { get; set; }
    public string PsyState { get; set; }
    public decimal RiskPercent { get; set; }
    public decimal RiskAmount { get; set; }
    public decimal RrPlanned { get; set; }
}

public class LogTradeCommandHandler : IRequestHandler<LogTradeCommand, Trade>
{
    private readonly IAppDbContext _context;

    public LogTradeCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Trade> Handle(LogTradeCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify the Daily Journal exists
        var journal = await _context.DailyJournals.FindAsync(new object[] { request.DailyJournalId }, cancellationToken);
        if (journal == null)
        {
            throw new Exception($"No daily journal found with ID {request.DailyJournalId}. Did you initialize the day?");
        }

        // 2. Map the Command to our Trade Entity
        var trade = new Trade
        {
            DailyJournalId = request.DailyJournalId,
            TradeNumber = request.TradeNumber,
            Pair = request.Pair,
            Exchange = request.Exchange,
            Direction = request.Direction,
            Setup = request.Setup,
            TimeFrame = request.TimeFrame,
            EntryThesis = request.EntryThesis,
            Context = request.Context,
            EntryPlanned = request.EntryPlanned,
            Sl = request.Sl,
            Tp = request.Tp,
            Size = request.Size,
            Leverage = request.Leverage,
            EntryActual = request.EntryActual,
            EntryTime = request.EntryTime,
            PsyState = request.PsyState,
            RiskPercent = request.RiskPercent,
            RiskAmount = request.RiskAmount,
            RrPlanned = request.RrPlanned
            // Notice: We don't set Exit, PlAmount, Outcome, etc. because the trade is active!
        };

        // 3. Increment the daily total trades counter
        // journal.TotalTrades += 1;

        // 4. Save to database
        _context.Trades.Add(trade);
        await _context.SaveChangesAsync(cancellationToken);

        return trade;
    }
}
