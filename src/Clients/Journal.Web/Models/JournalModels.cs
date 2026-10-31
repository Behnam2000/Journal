using System.ComponentModel.DataAnnotations;

namespace Journal.Web.Models;

public class DailyJournalDto
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public decimal StartingBalance { get; set; }
}

public class CreateJournalRequest
{
    [Required]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Starting balance must be greater than zero")]
    public decimal StartingBalance { get; set; }

}