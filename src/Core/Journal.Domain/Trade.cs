using System.ComponentModel.DataAnnotations.Schema;
using Journal.Domain.Common;

namespace Journal.Domain.Entities;

public class Trade : IUserOwnedEntity
{
    public long Id { get; set; }
    public int TradeNumber { get; set; }
    public string Pair { get; set; }
    public string Exchange { get; set; }
    public string Direction { get; set; }
    public string Setup { get; set; }
    public string TimeFrame { get; set; }
    public string EntryThesis { get; set; }
    public string Context { get; set; }

    // Entry metrics
    public decimal EntryPlanned { get; set; }
    public decimal Sl { get; set; }
    public decimal Tp { get; set; }
    public decimal Size { get; set; }
    public decimal Leverage { get; set; }
    public decimal EntryActual { get; set; }
    public DateTimeOffset EntryTime { get; set; }

    // Exit metrics
    public decimal? Exit { get; set; }
    public DateTimeOffset? ExitTime { get; set; }
    public string? ExitReason { get; set; }
    public decimal? Fees { get; set; }

    // Psychology & Rules
    public string PsyState { get; set; }
    public bool? FollowedRules { get; set; }

    // Post-trade review (Nullable)
    public string? Mistakes { get; set; }
    public string? WentWell { get; set; }
    public string? TakeAways { get; set; }
    public string? ChartRf { get; set; }
    public string? Tags { get; set; }

    // Risk math
    public decimal RiskPercent { get; set; }
    public decimal RiskAmount { get; set; }
    public decimal RrPlanned { get; set; }

    // P&L Math
    public decimal? PlAmount { get; set; }
    public decimal? PlPercent { get; set; }
    public decimal? RealizedR { get; set; }
    public string? Outcome { get; set; }

    // Relation Many(Trade) To ONE(DailyJournal) - Foreign Key
    public long DailyJournalId { get; set; }
    public DailyJournal? DailyJournal { get; set; }

    [NotMapped]
    public string Status => ExitTime.HasValue ? "Closed" : "Open";

    public string UserId { get; set; } = string.Empty;
}