namespace ShivohumExpenseTracker.Api.Models.Entities;

public class Project
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? StartDate { get; set; }
    public decimal? ContractAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOn { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<ClientPayment> ClientPayments { get; set; } = new List<ClientPayment>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
