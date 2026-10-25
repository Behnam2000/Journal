using MediatR;
using Microsoft.AspNetCore.Mvc;
using Journal.Application.Trades.Queries;
using Journal.Application.Trades.Commands;

namespace Journal.Api.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly IMediator _mediator;

    public TradesController(IMediator mediator)
    {
        _mediator = mediator;
    }


    // GET: api/trades
    // Fetches all trades in the database
    [HttpGet]
    public async Task<IActionResult> GetAllTrades()
    {
        var trades = await _mediator.Send(new GetAllTradesQuery());
        return Ok(trades);
    }

    // GET: api/trades/{id}
    // Fetches a single specific trade by its ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTradeById(long id)
    {
        var trade = await _mediator.Send(new GetTradeByIdQuery { Id = id });

        if (trade == null) return NotFound("Trade not found");

        return Ok(trade);
    }


    // POST : api/trades
    // Logs a new trade entry (Active Position)
    [HttpPost]
    public async Task<IActionResult> LogTrade([FromBody] OpenTradeDto request)
    {
        try
        {
            var command = new LogTradeCommand
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
            };

            var trade = await _mediator.Send(command);
            return Ok(trade);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PUT : api/trades/{id}/close
    // Updates an active position with the exit results
    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseTrade(long id, [FromBody] CloseTradeDto request)
    {
        try
        {
            var command = new CloseTradeCommand
            {
                Id = id,
                Exit = request.Exit,
                ExitTime = request.ExitTime,
                ExitReason = request.ExitReason,
                Fees = request.Fees,
                PlAmount = request.PlAmount,
                PlPercent = request.PlPercent,
                RealizedR = request.RealizedR,
                Outcome = request.Outcome,
                FollowedRules = request.FollowedRules
            };

            var trade = await _mediator.Send(command);
            return Ok(trade);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PUT: api/trades/{id}
    // Updates the details of an existing open trade
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOpenTrade(long id, [FromBody] UpdateOpenTradeDto request)
    {
        try
        {
            var command = new UpdateOpenTradeCommand
            {
                Id = id,
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
                PsyState = request.PsyState,
                RiskPercent = request.RiskPercent,
                RiskAmount = request.RiskAmount,
                RrPlanned = request.RrPlanned
            };

            var trade = await _mediator.Send(command);
            return Ok(trade);
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
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