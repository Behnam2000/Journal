using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> b) =>
        b.HasOne(t => t.DailyJournal).WithMany(j => j.Trades)
        .HasForeignKey(t => t.DailyJournalId);
}