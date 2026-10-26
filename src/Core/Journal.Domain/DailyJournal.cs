namespace Journal.Domain.Entities;

using System.ComponentModel.DataAnnotations.Schema;

public class DailyJournal
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public string Day { get; set; }
    public decimal StartingBalance { get; set; }
    public decimal? EndingBalance { get; set; }
    public string? OverallTrend { get; set; }
    public string? Volatility { get; set; }
    public string? MpState { get; set; }
    public string? News { get; set; }
    public string? Notes { get; set; }
    public short TotalTrades { get; set; } = 0;
    public short Wins { get; set; } = 0;
    public short Losses { get; set; } = 0;
    public decimal? NetPlAmount { get; set; }
    public decimal? WinRate { get; set; }
    public decimal? BestTradeAmount { get; set; }
    public string? BestTradePair { get; set; }
    public decimal? WorstTradeAmount { get; set; }
    public string? WorstTradePair { get; set; }
    public string? FollowedPlanOv { get; set; }
    public string? Lesson { get; set; }
    public string? ToImprove { get; set; }

    // Relation ONE(DailyJournal) To Many(Trade) - Primary Key
    public ICollection<Trade> Trades { get; set; } = new List<Trade>();


    [NotMapped]
    public string Status => NetPlAmount.HasValue ? "Closed" : "Open";

}