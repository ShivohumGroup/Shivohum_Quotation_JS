using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ShivohumExpenseTracker.Api.Data;
using ShivohumExpenseTracker.Api.Models.Dtos;
using ShivohumExpenseTracker.Api.Models.Entities;

namespace ShivohumExpenseTracker.Api.Services;

public class ExcelImportService(AppDbContext db)
{
    public async Task<ImportResultDto> ImportAsync(Stream excelStream, int userId, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook(excelStream);
        var sheet = workbook.Worksheets.First();
        var used = sheet.RangeUsed();
        if (used is null)
        {
            return new ImportResultDto { Errors = ["The Excel file is empty."] };
        }

        var headerRow = used.FirstRow();
        var map = BuildHeaderMap(headerRow);

        if (!map.ContainsKey("date") || !map.ContainsKey("type") || !map.ContainsKey("amount"))
        {
            return new ImportResultDto
            {
                Errors = ["Could not find Date, Type, and Amount columns. Use the same headers as the Excel tracker."]
            };
        }

        var categories = await db.Categories.ToListAsync(cancellationToken);
        var projects = await db.Projects.ToListAsync(cancellationToken);
        var paymentModes = await db.PaymentModes.ToListAsync(cancellationToken);
        var accounts = await db.Accounts.ToListAsync(cancellationToken);

        var imported = 0;
        var skipped = 0;
        var errors = new List<string>();
        var toAdd = new List<Transaction>();

        foreach (var row in used.RowsUsed().Skip(1))
        {
            var excelRow = row.RowNumber();
            try
            {
                var type = NormalizeType(GetCell(row, map, "type"));
                if (string.IsNullOrWhiteSpace(type))
                {
                    skipped++;
                    continue;
                }

                var amount = ParseAmount(GetCell(row, map, "amount"));
                if (amount is null or <= 0)
                {
                    errors.Add($"Row {excelRow}: invalid amount.");
                    skipped++;
                    continue;
                }

                var date = ParseDate(GetCell(row, map, "date"));
                if (date is null)
                {
                    errors.Add($"Row {excelRow}: invalid date.");
                    skipped++;
                    continue;
                }

                var categoryName = GetCell(row, map, "category");
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    categoryName = type == "Income" ? "Other Income" : "Other";
                }

                var category = categories.FirstOrDefault(c =>
                    c.Type.Equals(type, StringComparison.OrdinalIgnoreCase) &&
                    c.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

                if (category is null)
                {
                    category = new Category { CategoryName = categoryName.Trim(), Type = type, IsActive = true };
                    db.Categories.Add(category);
                    await db.SaveChangesAsync(cancellationToken);
                    categories.Add(category);
                }

                var projectName = GetCell(row, map, "project");
                Project? project = null;
                if (!string.IsNullOrWhiteSpace(projectName))
                {
                    project = projects.FirstOrDefault(p =>
                        p.ProjectName.Equals(projectName.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (project is null)
                    {
                        project = new Project
                        {
                            ProjectName = projectName.Trim(),
                            Status = "Active",
                            IsActive = true,
                            CreatedOn = DateTime.UtcNow
                        };
                        db.Projects.Add(project);
                        await db.SaveChangesAsync(cancellationToken);
                        projects.Add(project);
                    }
                }

                var modeName = GetCell(row, map, "paymentmode");
                if (string.IsNullOrWhiteSpace(modeName))
                {
                    modeName = "Cash";
                }

                var paymentMode = paymentModes.FirstOrDefault(p =>
                    p.PaymentModeName.Equals(modeName.Trim(), StringComparison.OrdinalIgnoreCase));
                if (paymentMode is null)
                {
                    paymentMode = new PaymentMode { PaymentModeName = modeName.Trim(), IsActive = true };
                    db.PaymentModes.Add(paymentMode);
                    await db.SaveChangesAsync(cancellationToken);
                    paymentModes.Add(paymentMode);
                }

                var notes = GetCell(row, map, "notes");
                Account? account = null;
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    account = accounts.FirstOrDefault(a =>
                        notes.Contains(a.AccountName, StringComparison.OrdinalIgnoreCase) ||
                        a.AccountName.Equals(notes.Trim(), StringComparison.OrdinalIgnoreCase));
                }

                var description = GetCell(row, map, "description");
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = category.CategoryName;
                }

                toAdd.Add(new Transaction
                {
                    TransactionDate = date.Value.Date,
                    Type = type,
                    CategoryId = category.CategoryId,
                    Description = description.Trim(),
                    ProjectId = project?.ProjectId,
                    PaymentModeId = paymentMode.PaymentModeId,
                    Amount = amount.Value,
                    AccountId = account?.AccountId,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow
                });
                imported++;
            }
            catch (Exception ex)
            {
                skipped++;
                errors.Add($"Row {excelRow}: {ex.Message}");
            }
        }

        if (toAdd.Count > 0)
        {
            db.Transactions.AddRange(toAdd);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new ImportResultDto
        {
            ImportedCount = imported,
            SkippedCount = skipped,
            Errors = errors.Take(25).ToList()
        };
    }

    private static Dictionary<string, int> BuildHeaderMap(IXLRangeRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var key = NormalizeHeader(cell.GetString());
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            map[key] = cell.Address.ColumnNumber;
        }

        return map;
    }

    private static string NormalizeHeader(string header)
    {
        var h = header.Trim().ToLowerInvariant()
            .Replace("₹", "")
            .Replace("(rs)", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("/", " ")
            .Replace("_", " ")
            .Replace("-", " ");

        h = string.Join(" ", h.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        if (h is "date" or "transaction date") return "date";
        if (h is "type") return "type";
        if (h is "category") return "category";
        if (h is "description" or "particulars" or "details") return "description";
        if (h is "client project" or "project" or "client" or "project client" or "project name") return "project";
        if (h is "payment mode" or "paymentmode" or "mode") return "paymentmode";
        if (h is "amount" or "amount ") return "amount";
        if (h is "notes" or "note" or "remark" or "remarks") return "notes";
        return h;
    }

    private static string GetCell(IXLRangeRow row, Dictionary<string, int> map, string key)
    {
        if (!map.TryGetValue(key, out var col))
        {
            return string.Empty;
        }

        var cell = row.Cell(col);
        if (cell.DataType == XLDataType.DateTime)
        {
            return cell.GetDateTime().ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        }

        if (cell.DataType == XLDataType.Number)
        {
            return cell.GetDouble().ToString(CultureInfo.InvariantCulture);
        }

        return cell.GetString().Trim();
    }

    private static string NormalizeType(string value)
    {
        var v = value.Trim();
        if (v.Equals("Income", StringComparison.OrdinalIgnoreCase)) return "Income";
        if (v.Equals("Expense", StringComparison.OrdinalIgnoreCase)) return "Expense";
        return string.Empty;
    }

    private static DateTime? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var formats = new[] { "dd-MM-yyyy", "d-M-yyyy", "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "MM/dd/yyyy" };
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            return exact;
        }

        if (DateTime.TryParse(value, CultureInfo.GetCultureInfo("en-IN"), DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var oa))
        {
            try
            {
                return DateTime.FromOADate(oa);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static decimal? ParseAmount(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = value.Replace("₹", "").Replace(",", "").Trim();
        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
        {
            return amount;
        }

        if (decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.GetCultureInfo("en-IN"), out amount))
        {
            return amount;
        }

        return null;
    }
}
