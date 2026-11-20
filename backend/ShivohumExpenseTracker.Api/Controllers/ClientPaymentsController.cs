using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShivohumExpenseTracker.Api.Data;
using ShivohumExpenseTracker.Api.Models.Dtos;
using ShivohumExpenseTracker.Api.Models.Entities;

namespace ShivohumExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:int}/payments")]
public class ClientPaymentsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClientPaymentDto>>> Get(int projectId)
    {
        if (!await db.Projects.AnyAsync(p => p.ProjectId == projectId))
        {
            return NotFound();
        }

        var items = await Query(projectId)
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.ClientPaymentId)
            .ToListAsync();

        return Ok(items.Select(Map).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<ClientPaymentDto>> Create(int projectId, ClientPaymentWriteRequest request)
    {
        if (!await db.Projects.AnyAsync(p => p.ProjectId == projectId))
        {
            return NotFound();
        }

        var error = await ValidateAsync(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        var entity = new ClientPayment
        {
            ProjectId = projectId,
            PaymentDate = request.PaymentDate.Date,
            Amount = request.Amount,
            PaymentModeId = request.PaymentModeId,
            AccountId = request.AccountId,
            ReferenceNo = string.IsNullOrWhiteSpace(request.ReferenceNo) ? null : request.ReferenceNo.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedBy = User.GetUserId(),
            CreatedOn = DateTime.UtcNow
        };

        db.ClientPayments.Add(entity);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { projectId, id = entity.ClientPaymentId }, await LoadDtoAsync(entity.ClientPaymentId));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientPaymentDto>> GetById(int projectId, int id)
    {
        var item = await Query(projectId).FirstOrDefaultAsync(p => p.ClientPaymentId == id);
        return item is null ? NotFound() : Ok(Map(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClientPaymentDto>> Update(int projectId, int id, ClientPaymentWriteRequest request)
    {
        var entity = await db.ClientPayments.FirstOrDefaultAsync(p => p.ProjectId == projectId && p.ClientPaymentId == id);
        if (entity is null)
        {
            return NotFound();
        }

        var error = await ValidateAsync(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        entity.PaymentDate = request.PaymentDate.Date;
        entity.Amount = request.Amount;
        entity.PaymentModeId = request.PaymentModeId;
        entity.AccountId = request.AccountId;
        entity.ReferenceNo = string.IsNullOrWhiteSpace(request.ReferenceNo) ? null : request.ReferenceNo.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entity.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(await LoadDtoAsync(id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int projectId, int id)
    {
        var entity = await db.ClientPayments.FirstOrDefaultAsync(p => p.ProjectId == projectId && p.ClientPaymentId == id);
        if (entity is null)
        {
            return NotFound();
        }

        db.ClientPayments.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<ClientPayment> Query(int projectId) =>
        db.ClientPayments
            .AsNoTracking()
            .Include(p => p.PaymentMode)
            .Include(p => p.Account)
            .Include(p => p.CreatedByUser)
            .Where(p => p.ProjectId == projectId);

    private async Task<ClientPaymentDto> LoadDtoAsync(int id)
    {
        var item = await db.ClientPayments
            .AsNoTracking()
            .Include(p => p.PaymentMode)
            .Include(p => p.Account)
            .Include(p => p.CreatedByUser)
            .FirstAsync(p => p.ClientPaymentId == id);

        return Map(item);
    }

    private async Task<string?> ValidateAsync(ClientPaymentWriteRequest request)
    {
        if (request.Amount <= 0)
        {
            return "Amount must be greater than zero.";
        }

        if (!await db.PaymentModes.AnyAsync(p => p.PaymentModeId == request.PaymentModeId && p.IsActive))
        {
            return "Payment mode was not found.";
        }

        if (request.AccountId.HasValue && !await db.Accounts.AnyAsync(a => a.AccountId == request.AccountId && a.IsActive))
        {
            return "Account was not found.";
        }

        return null;
    }

    private static ClientPaymentDto Map(ClientPayment p) => new()
    {
        ClientPaymentId = p.ClientPaymentId,
        ProjectId = p.ProjectId,
        PaymentDate = p.PaymentDate,
        Amount = p.Amount,
        PaymentModeId = p.PaymentModeId,
        PaymentModeName = p.PaymentMode.PaymentModeName,
        AccountId = p.AccountId,
        AccountName = p.Account?.AccountName,
        ReferenceNo = p.ReferenceNo,
        Notes = p.Notes,
        CreatedByName = p.CreatedByUser.FullName,
        CreatedOn = p.CreatedOn
    };
}
