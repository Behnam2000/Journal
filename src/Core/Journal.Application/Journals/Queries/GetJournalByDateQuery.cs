using Journal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Journal.Application.Journals.Queries;

public class GetJournalByDateQuery : IRequest<DailyJournal?>
{
    public DateOnly Date { get; set; }
}

public class GetJournalByDateQueryHandler : IRequestHandler<GetJournalByDateQuery, DailyJournal?>
{
    private readonly IAppDbContext _context;

    public GetJournalByDateQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DailyJournal?> Handle(GetJournalByDateQuery request, CancellationToken cancellationToken)
    {
        return await _context.DailyJournals
            .AsNoTracking()
            .Include(j => j.Trades)
            .FirstOrDefaultAsync(j => j.Date == request.Date, cancellationToken);
    }
}
