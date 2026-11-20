namespace ShivohumExpenseTracker.Api.Models.Entities;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<ClientPayment> ClientPayments { get; set; } = new List<ClientPayment>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
