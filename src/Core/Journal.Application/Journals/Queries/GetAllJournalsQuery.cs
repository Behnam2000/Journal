using Journal.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Journal.Application.Journals.Queries;

public class GetAllJournalsQuery : IRequest<List<DailyJournal>>
{
}

public class GetAllJournalsQueryHandler : IRequestHandler<GetAllJournalsQuery, List<DailyJournal>>
{
    private readonly IAppDbContext _context;

    public GetAllJournalsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DailyJournal>> Handle(GetAllJournalsQuery request, CancellationToken cancellationToken)
    {
        return await _context.DailyJournals
            .AsNoTracking()
            .OrderByDescending(j => j.Date)
            .ToListAsync(cancellationToken);
    }
}
