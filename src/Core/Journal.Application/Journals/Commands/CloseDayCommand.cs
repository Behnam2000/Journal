using Journal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Journal.Application.Common.Interfaces;

namespace Journal.Application.Journals.Commands;

// 1. The Command (Replaces CloseDayDTO)
public class CloseDayCommand : IRequest<DailyJournal>
{
    public long Id { get; set; }
    public string? OverallTrend { get; set; }
    public string? Volatility { get; set; }
    public string? MpState { get; set; }
    public string? News { get; set; }
    public string? Notes { get; set; }
    public string? FollowedPlanOv { get; set; }
    public string? Lesson { get; set; }
    public string? ToImprove { get; set; }
}

//2. The Handler (Replaces the logic in the Controller)
public class CloseDayCommandHandler : IRequestHandler<CloseDayCommand, DailyJournal>
{
    private readonly IAppDbContext _context;

    public CloseDayCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DailyJournal> Handle(CloseDayCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch the journal AND explicitly load the trades to do the math
        var journal = await _context.DailyJournals
            .Include(j => j.Trades)
            .FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);
        if (journal == null) throw new Exception("Journal not found"); // Or handle via a custom Exception

        // 2. Filter to only look at closed trades
        var closedTrades = journal.Trades.Where(t => t.ExitTime.HasValue).ToList();

        // 3. Recalculate Counters (ensures 100% accuracy)
        //All your math logic lives securely in the Application layer now!
        journal.TotalTrades = (short)closedTrades.Count;
        journal.Wins = (short)closedTrades.Count(t => t.Outcome == "Win");
        journal.Losses = (short)closedTrades.Count(t => t.Outcome == "Loss");

        // 4. Calculate Win Rate (%)
        journal.WinRate = journal.TotalTrades > 0
            ? Math.Round((decimal)journal.Wins / journal.TotalTrades * 100, 2) : 0;

        var grossPl = closedTrades.Sum(t => t.PlAmount ?? 0);
        var totalFees = closedTrades.Sum(t => t.Fees ?? 0);

        // 5. Calculate P&L and Balances
        // journal.NetPlAmount = grossPl - totalFees;
        journal.EndingBalance = journal.StartingBalance + journal.NetPlAmount;

        // 6. Find Best and Worst Trades
        if (closedTrades.Any())
        {
            var bestTrade = closedTrades.OrderByDescending(t => t.PlAmount).First();
            journal.BestTradeAmount = bestTrade.PlAmount;
            journal.BestTradePair = bestTrade.Pair;

            var worstTrade = closedTrades.OrderBy(t => t.PlAmount).First();
            journal.WorstTradeAmount = worstTrade.PlAmount;
            journal.WorstTradePair = worstTrade.Pair;
        }

        // 7. Save the Qualitative Review from the user
        journal.OverallTrend = request.OverallTrend;
        journal.Volatility = request.Volatility;
        journal.MpState = request.MpState;
        journal.News = request.News;
        journal.Notes = request.Notes;
        journal.FollowedPlanOv = request.FollowedPlanOv;
        journal.Lesson = request.Lesson;
        journal.ToImprove = request.ToImprove;

        await _context.SaveChangesAsync(cancellationToken);

        return journal;
    }
}

