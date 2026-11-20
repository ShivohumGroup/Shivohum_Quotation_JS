namespace ShivohumExpenseTracker.Api.Models.Entities;

public class Document
{
    public int DocumentId { get; set; }
    public string Kind { get; set; } = "Quotation";
    public string Title { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string PayloadJson { get; set; } = "{}";
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }

    public Project? Project { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
