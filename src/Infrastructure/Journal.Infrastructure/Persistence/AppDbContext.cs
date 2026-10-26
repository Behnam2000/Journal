using Journal.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using Journal.Application.Common.Interfaces;
=======
>>>>>>> devin/1792968709-step2-layering

namespace Journal.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DailyJournal> DailyJournals { get; set; }

    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
<<<<<<< HEAD
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppContext).Assembly);

=======
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
>>>>>>> devin/1792968709-step2-layering
    }
}
