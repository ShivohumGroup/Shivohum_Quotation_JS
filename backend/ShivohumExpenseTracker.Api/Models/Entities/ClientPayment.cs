namespace ShivohumExpenseTracker.Api.Models.Entities;

public class ClientPayment
{
    public int ClientPaymentId { get; set; }
    public int ProjectId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public int PaymentModeId { get; set; }
    public int? AccountId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }

    public Project Project { get; set; } = null!;
    public PaymentMode PaymentMode { get; set; } = null!;
    public Account? Account { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
