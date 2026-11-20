namespace ShivohumExpenseTracker.Api.Models.Entities;

public class PaymentMode
{
    public int PaymentModeId { get; set; }
    public string PaymentModeName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<ClientPayment> ClientPayments { get; set; } = new List<ClientPayment>();
}
