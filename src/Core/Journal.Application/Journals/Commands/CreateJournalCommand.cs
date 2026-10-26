using Journal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Journal.Application.Journals.Commands;

// Returns null when a journal already exists for the date.
public class CreateJournalCommand : IRequest<DailyJournal?>
{
    public DateOnly Date { get; set; }
    public decimal StartingBalance { get; set; }
}

public class CreateJournalCommandHandler : IRequestHandler<CreateJournalCommand, DailyJournal?>
{
    private readonly IAppDbContext _context;

    public CreateJournalCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DailyJournal?> Handle(CreateJournalCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.DailyJournals.AnyAsync(j => j.Date == request.Date, cancellationToken);
        if (exists)
        {
            return null;
        }

        var journal = new DailyJournal
        {
            Date = request.Date,
            Day = request.Date.DayOfWeek.ToString(),
            StartingBalance = request.StartingBalance
        };

        _context.DailyJournals.Add(journal);
        await _context.SaveChangesAsync(cancellationToken);

        return journal;
    }
}
