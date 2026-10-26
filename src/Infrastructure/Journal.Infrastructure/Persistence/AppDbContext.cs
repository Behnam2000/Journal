using Microsoft.EntityFrameworkCore;
using Journal.Application.Common.Interfaces;

namespace Journal.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<DailyJournal> DailyJournals { get; set; }

    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppContext).Assembly);

    }
}