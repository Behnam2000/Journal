using Journal.Application.Journals.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Journal.Api.Controllers;

[ApiController]
[Route("api/daily-journals")]
public class DailyJournalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DailyJournalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // PUT: api/daily-journals/{id}/close
    // Calculates the day's final math and saves the qualitative review
    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseDay(long id, [FromBody] CloseDayCommand command)
    {
        // Ensure the ID in the URL matches the command
        command.Id = id;

        // Send the command to the Application layer
        var journal = await _mediator.Send(command);

        return Ok(journal);

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



}

// A simple Data Transfer Object (DTO) to control what the user sends in the POST request
public class CreateJournalDto
{
    public DateOnly Date { get; set; }
    public decimal StartingBalance { get; set; }
}


