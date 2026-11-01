using System.ComponentModel.DataAnnotations;

namespace Journal.Web.Models;

public class TradeDto
{
    public long Id { get; set; }
    public int DailyJournalId { get; set; }
    public string Pair { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal EntryActual { get; set; }
    public decimal? Exit { get; set; }
    public decimal? PlAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateTradeRequest
{
    public long DailyJournalId { get; set; }

    [Required] public string Pair { get; set; } = string.Empty;
    [Required] public string Exchange { get; set; } = string.Empty;
    [Required] public string Direction { get; set; } = "Long";
    [Required] public string Setup { get; set; } = string.Empty;
    [Required] public string TimeFrame { get; set; } = string.Empty;

    // Entry Metrics
    public decimal EntryPlanned { get; set; }
    public decimal Sl { get; set; }
    public decimal Tp { get; set; }
    public decimal Size { get; set; }
    public decimal Leverage { get; set; }
    public decimal EntryActual { get; set; }
    public DateTimeOffset EntryTime { get; set; } = DateTimeOffset.UtcNow;

    // Psychology & Thesis
    [Required] public string PsyState { get; set; } = string.Empty;
    [Required] public string EntryThesis { get; set; } = string.Empty;
    [Required] public string Context { get; set; } = string.Empty;

    // Risk Math
    public decimal RiskPercent { get; set; }
    public decimal RiskAmount { get; set; }
    public decimal RrPlanned { get; set; }
}