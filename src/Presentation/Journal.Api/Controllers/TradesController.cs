using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Journal.Api.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly AppDbContext _context;

    public TradesController(AppDbContext context)
    {
        _context = context;
    }


    // GET: api/trades
    // Fetches all trades in the database
    [HttpGet]
    public async Task<IActionResult> GetAllTrades()
    {
        var trades = await _context.Trades
            .OrderByDescending(t => t.EntryTime)
            .ToListAsync();

        return Ok(trades);
    }

    // GET: api/trades/{id}
    // Fetches a single specific trade by its ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTradeById(long id)
    {
        var trade = await _context.Trades.FindAsync(id);

        if (trade == null) return NotFound("Trade not found");

        return Ok(trade);
    }


    // POST : api/trades
    // Logs a new trade entry (Active Position)
    [HttpPost]
    public async Task<IActionResult> LogTrade([FromBody] OpenTradeDto request)
    {
        // 1. Verify the Daily Journal exists
        var journal = await _context.DailyJournals.FindAsync(request.DailyJournalId);
        if (journal == null)
        {
            return NotFound($"No daily journal found with ID {request.DailyJournalId}. Did you initialize the day?");
        }

        // 2.Map the DTO to our Trade Entity
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
        journal.TotalTrades += 1;

        // 4. Save to database
        _context.Trades.Add(trade);
        await _context.SaveChangesAsync();

        return Ok(trade);
    }

    // PUT : api/trades/{id}/close
    // Updates an active position with the exit results
    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseTrade(long id, [FromBody] CloseTradeDto request)
    {
        // 1. Find the active trade and include its parent journal
        var trade = await _context.Trades
            .Include(t => t.DailyJournal)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trade == null) return NotFound("Trade Not Found");

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

        // ! subtract the fee directly from the sumbitted P&L for that specific trade
        trade.PlAmount = request.PlAmount - request.Fees;
        trade.PlAmount = request.PlPercent;

        // 3. Update the daily journal Win/Loss counters based on the outcome
        if (request.Outcome.Equals("Win", StringComparison.OrdinalIgnoreCase))
        {
            trade.DailyJournal.Wins += 1;
        }
        else if (request.Outcome.Equals("Loss", StringComparison.OrdinalIgnoreCase))
        {
            trade.DailyJournal.Losses += 1;
        }

        await _context.SaveChangesAsync();

        return Ok(trade);
    }

    // PUT: api/trades/{id}
    // Updates the details of an existing open trade
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOpenTrade(long id, [FromBody] UpdateOpenTradeDto request)
    {
        var trade = await _context.Trades.FindAsync(id);

        if (trade == null) return NotFound("Trade not found.");

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

        await _context.SaveChangesAsync();

        return Ok(trade);
    }


}

// --- Data Transfer Objects (DTOs) ---

public class OpenTradeDto
{
    public long DailyJournalId { get; set; } // Links to the day
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

public class CloseTradeDto
{
    public decimal Exit { get; set; }
    public DateTimeOffset ExitTime { get; set; }
    public string ExitReason { get; set; }
    public decimal Fees { get; set; }
    public decimal PlAmount { get; set; }
    public decimal PlPercent { get; set; }
    public decimal RealizedR { get; set; }
    public string Outcome { get; set; } // "Win" or "Loss"
    public bool FollowedRules { get; set; }
}

public class UpdateOpenTradeDto
{
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