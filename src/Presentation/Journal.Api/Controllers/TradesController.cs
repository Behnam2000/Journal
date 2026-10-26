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
    public async Task<IActionResult> LogTrade([FromBody] LogTradeCommand command)
    {
        var trade = await _mediator.Send(command);
        return Ok(trade);

    }

    // PUT : api/trades/{id}/close
    // Updates an active position with the exit results
    [HttpPut("{id}/close")]
    public async Task<IActionResult> CloseTrade(long id, [FromBody] CloseTradeCommand command)
    {
        command.Id = id; // Ensure the ID from the URL is attached to the command
        var trade = await _mediator.Send(command);
        return Ok(trade);
    }

    // PUT: api/trades/{id}
    // Updates the details of an existing open trade
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOpenTrade(long id, [FromBody] UpdateOpenTradeCommand command)
    {
        command.Id = id;
        var trade = await _mediator.Send(command);
        return Ok(trade);
    }

}