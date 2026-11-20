using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShivohumExpenseTracker.Api.Data;
using ShivohumExpenseTracker.Api.Models.Dtos;
using ShivohumExpenseTracker.Api.Models.Entities;

namespace ShivohumExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TransactionDto>>> Get(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? projectId,
        [FromQuery] string? type,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = db.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Project)
            .Include(t => t.PaymentMode)
            .Include(t => t.Account)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (from.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= from.Value.Date);
        }

        if (to.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= to.Value.Date);
        }

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId);
        }

        if (!string.IsNullOrWhiteSpace(type) && (type == "Income" || type == "Expense"))
        {
            query = query.Where(t => t.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t =>
                t.Description.Contains(term) ||
                (t.Notes != null && t.Notes.Contains(term)) ||
                t.Category.CategoryName.Contains(term) ||
                (t.Project != null && t.Project.ProjectName.Contains(term)));
        }

        var total = await query.CountAsync();
        var rows = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.TransactionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PagedResult<TransactionDto>
        {
            Items = rows.Select(Map).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransactionDto>> GetById(int id)
    {
        var item = await db.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Project)
            .Include(t => t.PaymentMode)
            .Include(t => t.Account)
            .Include(t => t.CreatedByUser)
            .FirstOrDefaultAsync(t => t.TransactionId == id);

        return item is null ? NotFound() : Ok(Map(item));
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create(TransactionWriteRequest request)
    {
        var error = await ValidateAsync(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        var entity = new Transaction
        {
            TransactionDate = request.TransactionDate.Date,
            Type = NormalizeType(request.Type),
            CategoryId = request.CategoryId,
            Description = request.Description.Trim(),
            ProjectId = request.ProjectId,
            PaymentModeId = request.PaymentModeId,
            Amount = request.Amount,
            AccountId = request.AccountId,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedBy = User.GetUserId(),
            CreatedOn = DateTime.UtcNow
        };

        db.Transactions.Add(entity);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.TransactionId }, await LoadDtoAsync(entity.TransactionId));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransactionDto>> Update(int id, TransactionWriteRequest request)
    {
        var entity = await db.Transactions.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        var error = await ValidateAsync(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        entity.TransactionDate = request.TransactionDate.Date;
        entity.Type = NormalizeType(request.Type);
        entity.CategoryId = request.CategoryId;
        entity.Description = request.Description.Trim();
        entity.ProjectId = request.ProjectId;
        entity.PaymentModeId = request.PaymentModeId;
        entity.Amount = request.Amount;
        entity.AccountId = request.AccountId;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entity.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(await LoadDtoAsync(id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Transactions.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        db.Transactions.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateAsync(TransactionWriteRequest request)
    {
        var type = NormalizeType(request.Type);
        if (type is not ("Income" or "Expense"))
        {
            return "Type must be Income or Expense.";
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return "Description is required.";
        }

        if (request.Amount <= 0)
        {
            return "Amount must be greater than zero.";
        }

        var category = await db.Categories.FindAsync(request.CategoryId);
        if (category is null || !category.IsActive)
        {
            return "Category was not found.";
        }

        if (!category.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
        {
            return "Category does not match the selected type.";
        }

        if (request.ProjectId.HasValue && !await db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId))
        {
            return "Project was not found.";
        }

        if (!await db.PaymentModes.AnyAsync(p => p.PaymentModeId == request.PaymentModeId))
        {
            return "Payment mode was not found.";
        }

        if (request.AccountId.HasValue && !await db.Accounts.AnyAsync(a => a.AccountId == request.AccountId))
        {
            return "Account was not found.";
        }

        return null;
    }

    private static string NormalizeType(string type) =>
        type.Equals("Income", StringComparison.OrdinalIgnoreCase) ? "Income" :
        type.Equals("Expense", StringComparison.OrdinalIgnoreCase) ? "Expense" :
        type.Trim();

    private async Task<TransactionDto> LoadDtoAsync(int id)
    {
        var item = await db.Transactions
            .AsNoTracking()
            .Include(t => t.Category)
            .Include(t => t.Project)
            .Include(t => t.PaymentMode)
            .Include(t => t.Account)
            .Include(t => t.CreatedByUser)
            .FirstAsync(t => t.TransactionId == id);

        return Map(item);
    }

    private static TransactionDto Map(Transaction t) => new()
    {
        TransactionId = t.TransactionId,
        TransactionDate = t.TransactionDate,
        Type = t.Type,
        CategoryId = t.CategoryId,
        CategoryName = t.Category.CategoryName,
        Description = t.Description,
        ProjectId = t.ProjectId,
        ProjectName = t.Project?.ProjectName,
        PaymentModeId = t.PaymentModeId,
        PaymentModeName = t.PaymentMode.PaymentModeName,
        Amount = t.Amount,
        AccountId = t.AccountId,
        AccountName = t.Account?.AccountName,
        Notes = t.Notes,
        CreatedByName = t.CreatedByUser.FullName,
        CreatedOn = t.CreatedOn
    };
}
