using Microsoft.EntityFrameworkCore;
using Journal.Application;

public class AppDbContext : DbContext, IAppDbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<DailyJournal> DailyJournals { get; set; }

    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure One To Many: DailyJournal -> Trade
        modelBuilder.Entity<Trade>()
            .HasOne(t => t.DailyJournal)
            .WithMany(t => t.Trades)
            .HasForeignKey(t => t.DailyJournalId);
    }
}