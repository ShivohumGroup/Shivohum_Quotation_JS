namespace ShivohumExpenseTracker.Api.Models.Entities;

public class Account
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<ClientPayment> ClientPayments { get; set; } = new List<ClientPayment>();
}
