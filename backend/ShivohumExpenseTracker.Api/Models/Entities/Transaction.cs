namespace ShivohumExpenseTracker.Api.Models.Entities;

public class Transaction
{
    public int TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public int PaymentModeId { get; set; }
    public decimal Amount { get; set; }
    public int? AccountId { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }

    public Category Category { get; set; } = null!;
    public Project? Project { get; set; }
    public PaymentMode PaymentMode { get; set; } = null!;
    public Account? Account { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
