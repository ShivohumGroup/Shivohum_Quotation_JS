using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ShivohumExpenseTracker.Api.Data;
using ShivohumExpenseTracker.Api.Models.Dtos;

namespace ShivohumExpenseTracker.Api.Services;

public class DashboardService(AppDbContext db)
{
    public async Task<DashboardDto> GetAsync(string? preset, DateTime? from, DateTime? to)
    {
        var range = ResolveRange(preset, from, to);
        var filtered = db.Transactions.AsNoTracking()
            .Where(t => t.TransactionDate >= range.From && t.TransactionDate <= range.To);

        var monthSpanFrom = range.Preset == "thisMonth"
            ? new DateTime(range.From.Year, 1, 1)
            : range.From;
        var monthSpanTo = range.Preset == "thisMonth"
            ? new DateTime(range.From.Year, 12, 31)
            : range.To;
        var monthQuery = range.Preset == "thisMonth"
            ? db.Transactions.AsNoTracking().Where(t => t.TransactionDate >= monthSpanFrom && t.TransactionDate <= monthSpanTo)
            : filtered;

        var totalIncome = await filtered.Where(t => t.Type == "Income").SumAsync(t => (decimal?)t.Amount) ?? 0;
        var totalExpense = await filtered.Where(t => t.Type == "Expense").SumAsync(t => (decimal?)t.Amount) ?? 0;

        var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var monthIncome = await db.Transactions.AsNoTracking()
            .Where(t => t.Type == "Income" && t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;
        var monthExpense = await db.Transactions.AsNoTracking()
            .Where(t => t.Type == "Expense" && t.TransactionDate >= monthStart && t.TransactionDate <= monthEnd)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return new DashboardDto
        {
            Preset = range.Preset,
            From = range.From,
            To = range.To,
            Summary = new DashboardSummaryDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetBalance = totalIncome - totalExpense,
                ThisMonthBalance = monthIncome - monthExpense
            },
            Projects = await GetProjectsAsync(filtered),
            Months = await GetMonthsAsync(monthQuery, monthSpanFrom, monthSpanTo),
            Categories = await GetCategoriesAsync(filtered)
        };
    }

    private async Task<IReadOnlyList<ProjectProfitDto>> GetProjectsAsync(IQueryable<Models.Entities.Transaction> filtered)
    {
        var grouped = await filtered
            .GroupBy(t => t.ProjectId)
            .Select(g => new
            {
                ProjectId = g.Key,
                Income = g.Where(t => t.Type == "Income").Sum(t => (decimal?)t.Amount) ?? 0,
                Expense = g.Where(t => t.Type == "Expense").Sum(t => (decimal?)t.Amount) ?? 0
            })
            .ToListAsync();

        var lifetimeExpense = await db.Transactions.AsNoTracking()
            .Where(t => t.Type == "Expense" && t.ProjectId != null)
            .GroupBy(t => t.ProjectId)
            .Select(g => new { ProjectId = g.Key, Expense = g.Sum(t => t.Amount) })
            .ToListAsync();

        var received = await db.ClientPayments.AsNoTracking()
            .GroupBy(p => p.ProjectId)
            .Select(g => new { ProjectId = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync();

        var projects = await db.Projects.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProjectName)
            .ToListAsync();

        var rows = projects.Select(project =>
        {
            var period = grouped.FirstOrDefault(g => g.ProjectId == project.ProjectId);
            var paid = received.FirstOrDefault(r => r.ProjectId == project.ProjectId)?.Total ?? 0;
            var expense = lifetimeExpense.FirstOrDefault(e => e.ProjectId == project.ProjectId)?.Expense ?? 0;
            return new ProjectProfitDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                ContractAmount = project.ContractAmount,
                ReceivedAmount = paid,
                DueAmount = ProjectFinance.Due(project.ContractAmount, paid),
                Income = period?.Income ?? 0,
                Expense = expense,
                Profit = paid - expense
            };
        }).ToList();

        var unassigned = grouped.FirstOrDefault(g => g.ProjectId == null);
        if (unassigned is not null && (unassigned.Income > 0 || unassigned.Expense > 0))
        {
            rows.Add(new ProjectProfitDto
            {
                ProjectId = null,
                ProjectName = "No project / office",
                ContractAmount = null,
                ReceivedAmount = 0,
                DueAmount = null,
                Income = unassigned.Income,
                Expense = unassigned.Expense,
                Profit = unassigned.Income - unassigned.Expense
            });
        }

        return rows.OrderByDescending(r => r.Profit).ToList();
    }

    private static async Task<IReadOnlyList<MonthlySummaryDto>> GetMonthsAsync(
        IQueryable<Models.Entities.Transaction> filtered,
        DateTime from,
        DateTime to)
    {
        var grouped = await filtered
            .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Income = g.Where(t => t.Type == "Income").Sum(t => (decimal?)t.Amount) ?? 0,
                Expense = g.Where(t => t.Type == "Expense").Sum(t => (decimal?)t.Amount) ?? 0
            })
            .ToListAsync();

        var lookup = grouped.ToDictionary(g => (g.Year, g.Month));
        var months = new List<MonthlySummaryDto>();
        var cursor = new DateTime(from.Year, from.Month, 1);
        var last = new DateTime(to.Year, to.Month, 1);

        while (cursor <= last)
        {
            lookup.TryGetValue((cursor.Year, cursor.Month), out var totals);
            var income = totals?.Income ?? 0;
            var expense = totals?.Expense ?? 0;
            months.Add(new MonthlySummaryDto
            {
                Year = cursor.Year,
                Month = cursor.Month,
                Label = cursor.ToString("MMM-yyyy", CultureInfo.GetCultureInfo("en-IN")),
                Income = income,
                Expense = expense,
                Balance = income - expense
            });
            cursor = cursor.AddMonths(1);
        }

        return months;
    }

    private async Task<IReadOnlyList<CategoryTotalDto>> GetCategoriesAsync(IQueryable<Models.Entities.Transaction> filtered)
    {
        var grouped = await filtered
            .Where(t => t.Type == "Expense")
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                Expense = g.Sum(t => (decimal?)t.Amount) ?? 0
            })
            .ToListAsync();

        var categories = await db.Categories.AsNoTracking()
            .Where(c => c.Type == "Expense" && c.IsActive)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        return categories.Select(category => new CategoryTotalDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Expense = grouped.FirstOrDefault(g => g.CategoryId == category.CategoryId)?.Expense ?? 0
        }).ToList();
    }

    private static (string Preset, DateTime From, DateTime To) ResolveRange(string? preset, DateTime? from, DateTime? to)
    {
        var today = DateTime.Today;
        var key = (preset ?? "thisYear").Trim().ToLowerInvariant();

        return key switch
        {
            "thismonth" => ("thisMonth", new DateTime(today.Year, today.Month, 1), today),
            "custom" when from.HasValue && to.HasValue =>
                ("custom", from.Value.Date, to.Value.Date < from.Value.Date ? from.Value.Date : to.Value.Date),
            _ => ("thisYear", new DateTime(today.Year, 1, 1), new DateTime(today.Year, 12, 31))
        };
    }
}
