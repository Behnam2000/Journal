using Microsoft.EntityFrameworkCore;

namespace Journal.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<DailyJournal> DailyJournals { get; }
    DbSet<Trade> Trades { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}