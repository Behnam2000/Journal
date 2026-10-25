using Journal.Application.Journals.Commands;
using Journal.Application.Journals.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> CloseDay(long id, [FromBody] CloseDayCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;

        var journal = await _mediator.Send(command, cancellationToken);

        return Ok(journal);
    }

    // POST: api/daily-journals
    // Initializes a new trading day in the morning
    [HttpPost]
    public async Task<IActionResult> CreateJournal([FromBody] CreateJournalCommand command, CancellationToken cancellationToken)
    {
        var journal = await _mediator.Send(command, cancellationToken);

        if (journal == null)
        {
            return BadRequest($"A journal for the date {command.Date} already exists");
        }

        return CreatedAtAction(nameof(GetJournalByDate), new { date = journal.Date.ToString("yyyy-MM-dd") }, journal);
    }

    // GET: api/daily-journals
    // Fetches your dashboard history
    [HttpGet]
    public async Task<IActionResult> GetAllJournals(CancellationToken cancellationToken)
    {
        var journals = await _mediator.Send(new GetAllJournalsQuery(), cancellationToken);

        return Ok(journals);
    }

    // GET: api/daily-journals/2026-10-10
    // Fetches a specific day and all of its trades
    [HttpGet("{date}")]
    public async Task<IActionResult> GetJournalByDate(DateOnly date, CancellationToken cancellationToken)
    {
        var journal = await _mediator.Send(new GetJournalByDateQuery { Date = date }, cancellationToken);

        if (journal == null)
        {
            return NotFound();
        }

        return Ok(journal);
    }
}
