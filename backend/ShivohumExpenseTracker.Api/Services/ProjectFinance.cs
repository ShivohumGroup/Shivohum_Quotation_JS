using ShivohumExpenseTracker.Api.Models.Dtos;
using ShivohumExpenseTracker.Api.Models.Entities;

namespace ShivohumExpenseTracker.Api.Services;

public static class ProjectFinance
{
    public static decimal? Due(decimal? contractAmount, decimal received) =>
        contractAmount.HasValue ? contractAmount.Value - received : null;

    public static ProjectDto ToDto(Project project, decimal received, decimal expense) => new()
    {
        ProjectId = project.ProjectId,
        ProjectName = project.ProjectName,
        ClientName = project.ClientName,
        Status = project.Status,
        StartDate = project.StartDate,
        ContractAmount = project.ContractAmount,
        ReceivedAmount = received,
        DueAmount = Due(project.ContractAmount, received),
        ExpenseAmount = expense,
        ProfitAmount = received - expense,
        Notes = project.Notes,
        IsActive = project.IsActive
    };
}
