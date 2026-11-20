using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShivohumExpenseTracker.Api.Data;
using ShivohumExpenseTracker.Api.Models.Dtos;

namespace ShivohumExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LookupsController(AppDbContext db) : ControllerBase
{
    [HttpGet("categories")]
    public async Task<ActionResult<IReadOnlyList<LookupDto>>> Categories([FromQuery] string? type)
    {
        var query = db.Categories.AsNoTracking().Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(c => c.Type == type);
        }

        var items = await query
            .OrderBy(c => c.CategoryName)
            .Select(c => new LookupDto { Id = c.CategoryId, Name = c.CategoryName, Extra = c.Type })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("payment-modes")]
    public async Task<ActionResult<IReadOnlyList<LookupDto>>> PaymentModes()
    {
        var items = await db.PaymentModes.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.PaymentModeName)
            .Select(p => new LookupDto { Id = p.PaymentModeId, Name = p.PaymentModeName })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("accounts")]
    public async Task<ActionResult<IReadOnlyList<LookupDto>>> Accounts()
    {
        var items = await db.Accounts.AsNoTracking()
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountName)
            .Select(a => new LookupDto { Id = a.AccountId, Name = a.AccountName, Extra = a.AccountType })
            .ToListAsync();

        return Ok(items);
    }
}
