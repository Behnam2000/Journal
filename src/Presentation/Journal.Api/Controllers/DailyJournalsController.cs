using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Journal.Api.Controllers;

[ApiController]
[Route("api/daily-journals")]
public class DailyJournalsController : ControllerBase
{
    private readonly AppDbContext _context;

    public DailyJournalsController(AppDbContext context)
    {
        _context = context;
    }

    // POST : api/daily-journals
    // Initializes a new trading day in the morning
    [HttpPost]
    public async Task<IActionResult> CreateJournal([FromBody] CreateJournalDto request)
    {
        // Prevent creating duplicate journals for the same day
        var exists = await _context.DailyJournals.AnyAsync(j => j.Date == request.Date);
        if (exists)
        {
            return BadRequest($"A journal for the date {request.Date} already exists");
        }

        var journal = new DailyJournal
        {
            Date = request.Date,
            Day = request.Date.DayOfWeek.ToString(), // Auto-calculates "Monday", "Tuesday", etc.
            StartingBalance = request.StartingBalance
        };

        _context.DailyJournals.Add(journal);
        await _context.SaveChangesAsync();

        // Returns a 201 Created response with the URL to fetch the new journal
        return CreatedAtAction(nameof(GetJournalByDate), new { date = journal.Date.ToString("yyyy-MM-dd") }, journal);
    }

    // GET : api/daily-journals
    // Fetches your dashboard history
    [HttpGet]
    public async Task<IActionResult> GetAllJournals()
    {
        var journals = await _context.DailyJournals
            .OrderByDescending(j => j.Date)
            .ToListAsync();

        return Ok(journals);
    }


    // GET: api/daily-journals/2026-10-10
    // Fetches a specific day AND all of the trades associated with it
    [HttpGet("{date}")]
    public async Task<IActionResult> GetJournalByDate(DateOnly date)
    {
        var journal = await _context.DailyJournals
            .Include(j => j.Trades)     // EF Core explicitly loads the related trades
            .FirstOrDefaultAsync(j => j.Date == date);

        if (journal == null)
        {
            return NotFound();
        }

        return Ok(journal);
    }


    // PUT: api/daily-journals/{id}/close
    // Calculates the day's final math and saves the qualitative review
    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseDay(long id, [FromBody] CloseDayDto request)
    {
        // 1. Fetch the journal AND explicitly load the trades to do the math
        var journal = await _context.DailyJournals
            .Include(j => j.Trades)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (journal == null) return NotFound();

        // 2. Filter to only look at closed trades
        var closedTrades = journal.Trades.Where(t => t.ExitTime.HasValue).ToList();

        // 3. Recalculate Counters (ensures 100% accuracy)
        journal.TotalTrades = (short)closedTrades.Count;
        journal.Wins = (short)closedTrades.Count(t => t.Outcome == "Win");
        journal.Losses = (short)closedTrades.Count(t => t.Outcome == "Loss");

        // 4. Calculate Win Rate (%)
        if (journal.TotalTrades > 0)
        {
            journal.WinRate = Math.Round((decimal)journal.Wins / journal.TotalTrades * 100, 2);
        }
        else
        {
            journal.WinRate = 0;
        }

        // 5. Calculate P&L and Balances
        journal.NetPlAmount = closedTrades.Sum(t => t.PlAmount ?? 0);
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

        await _context.SaveChangesAsync();

        return Ok(journal);
    }
}

// A simple Data Transfer Object (DTO) to control what the user sends in the POST request
public class CreateJournalDto
{
    public DateOnly Date { get; set; }
    public decimal StartingBalance { get; set; }
}

public class CloseDayDto
{
    public string OverallTrend { get; set; }
    public string Volatility { get; set; }
    public string MpState { get; set; }
    public string News { get; set; }
    public string Notes { get; set; }
    public string FollowedPlanOv { get; set; }
    public string Lesson { get; set; }
    public string ToImprove { get; set; }
}
