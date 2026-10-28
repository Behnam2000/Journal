using Journal.Application.Common.Interfaces;
using Journal.Domain.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Journal.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<IdentityUser>, IAppDbContext
{
    private readonly ICurrentUserService _currentUserService;


    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<DailyJournal> DailyJournals { get; set; }

    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Critical: This applies the Identity table configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // GLOBAL QUERY FILTERS: Automatically append "WHERE UserId = 'token_id'" to every single database query
        modelBuilder.Entity<DailyJournal>().HasQueryFilter(j => j.UserId == _currentUserService.UserId);
        modelBuilder.Entity<Trade>().HasQueryFilter(t => t.UserId == _currentUserService.UserId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // INTERCEPTOR: Before saving to PostgreSQL, automatically attach the UserId to any newly created rows
        var userId = _currentUserService.UserId;

        if (userId != null)
        {
            foreach (var entry in ChangeTracker.Entries<IUserOwnedEntity>().Where(e => e.State == EntityState.Added))
            {
                entry.Entity.UserId = userId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }


}
