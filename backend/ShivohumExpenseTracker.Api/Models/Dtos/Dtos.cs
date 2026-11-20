namespace ShivohumExpenseTracker.Api.Models.Dtos;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class TransactionDto
{
    public int TransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public int PaymentModeId { get; set; }
    public string PaymentModeName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? Notes { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}

public class TransactionWriteRequest
{
    public DateTime TransactionDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public int PaymentModeId { get; set; }
    public decimal Amount { get; set; }
    public int? AccountId { get; set; }
    public string? Notes { get; set; }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class ProjectDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? StartDate { get; set; }
    public decimal? ContractAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal? DueAmount { get; set; }
    public decimal ExpenseAmount { get; set; }
    public decimal ProfitAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class ProjectWriteRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? StartDate { get; set; }
    public decimal? ContractAmount { get; set; }
    public string? Notes { get; set; }
}

public class ClientPaymentDto
{
    public int ClientPaymentId { get; set; }
    public int ProjectId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public int PaymentModeId { get; set; }
    public string PaymentModeName { get; set; } = string.Empty;
    public int? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}

public class ClientPaymentWriteRequest
{
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public int PaymentModeId { get; set; }
    public int? AccountId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
}

public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Extra { get; set; }
}

public class ImportResultDto
{
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
    public IReadOnlyList<string> Errors { get; set; } = [];
}

public class DashboardDto
{
    public string Preset { get; set; } = "thisYear";
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DashboardSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<ProjectProfitDto> Projects { get; set; } = [];
    public IReadOnlyList<MonthlySummaryDto> Months { get; set; } = [];
    public IReadOnlyList<CategoryTotalDto> Categories { get; set; } = [];
}

public class DashboardSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }
    public decimal ThisMonthBalance { get; set; }
}

public class ProjectProfitDto
{
    public int? ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal? ContractAmount { get; set; }
    public decimal ReceivedAmount { get; set; }
    public decimal? DueAmount { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Profit { get; set; }
}

public class MonthlySummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal Balance { get; set; }
}

public class CategoryTotalDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Expense { get; set; }
}

public class DocumentListItemDto
{
    public int DocumentId { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? UpdatedOn { get; set; }
}

public class DocumentDto : DocumentListItemDto
{
    public string PayloadJson { get; set; } = "{}";
}

public class DocumentWriteRequest
{
    public string Kind { get; set; } = "Quotation";
    public string Title { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string PayloadJson { get; set; } = "{}";
}
