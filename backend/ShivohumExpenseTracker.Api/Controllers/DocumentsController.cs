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
public class DocumentsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentListItemDto>>> Get([FromQuery] string? kind)
    {
        var query = db.Documents.AsNoTracking().Include(d => d.Project).AsQueryable();
        if (kind is "Quotation" or "Invoice")
        {
            query = query.Where(d => d.Kind == kind);
        }

        var items = await query
            .OrderByDescending(d => d.UpdatedOn ?? d.CreatedOn)
            .Select(d => new DocumentListItemDto
            {
                DocumentId = d.DocumentId,
                Kind = d.Kind,
                Title = d.Title,
                ProjectId = d.ProjectId,
                ProjectName = d.Project != null ? d.Project.ProjectName : null,
                CreatedOn = d.CreatedOn,
                UpdatedOn = d.UpdatedOn
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDto>> GetById(int id)
    {
        var item = await db.Documents.AsNoTracking().Include(d => d.Project).FirstOrDefaultAsync(d => d.DocumentId == id);
        return item is null ? NotFound() : Ok(Map(item));
    }

    [HttpPost]
    public async Task<ActionResult<DocumentDto>> Create(DocumentWriteRequest request)
    {
        var error = await ValidateAsync(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        var entity = new Document
        {
            Kind = NormalizeKind(request.Kind),
            Title = TitleOrFallback(request),
            ProjectId = request.ProjectId,
            PayloadJson = string.IsNullOrWhiteSpace(request.PayloadJson) ? "{}" : request.PayloadJson,
            CreatedBy = User.GetUserId(),
            CreatedOn = DateTime.UtcNow
        };

        db.Documents.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.DocumentId }, await LoadAsync(entity.DocumentId));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DocumentDto>> Update(int id, DocumentWriteRequest request)
    {
        var entity = await db.Documents.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        var error = await ValidateAsync(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        entity.Kind = NormalizeKind(request.Kind);
        entity.Title = TitleOrFallback(request);
        entity.ProjectId = request.ProjectId;
        entity.PayloadJson = string.IsNullOrWhiteSpace(request.PayloadJson) ? "{}" : request.PayloadJson;
        entity.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(await LoadAsync(id));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Documents.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        db.Documents.Remove(entity);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string?> ValidateAsync(DocumentWriteRequest request)
    {
        if (NormalizeKind(request.Kind) is not ("Quotation" or "Invoice"))
        {
            return "Kind must be Quotation or Invoice.";
        }

        if (request.ProjectId.HasValue && !await db.Projects.AnyAsync(p => p.ProjectId == request.ProjectId))
        {
            return "Project was not found.";
        }

        return null;
    }

    private async Task<DocumentDto> LoadAsync(int id)
    {
        var item = await db.Documents.AsNoTracking().Include(d => d.Project).FirstAsync(d => d.DocumentId == id);
        return Map(item);
    }

    private static string NormalizeKind(string kind) =>
        kind.Equals("Invoice", StringComparison.OrdinalIgnoreCase) ? "Invoice" : "Quotation";

    private static string TitleOrFallback(DocumentWriteRequest request)
    {
        var title = (request.Title ?? "").Trim();
        return title.Length == 0 ? $"{NormalizeKind(request.Kind)} draft" : title[..Math.Min(title.Length, 200)];
    }

    private static DocumentDto Map(Document d) => new()
    {
        DocumentId = d.DocumentId,
        Kind = d.Kind,
        Title = d.Title,
        ProjectId = d.ProjectId,
        ProjectName = d.Project?.ProjectName,
        PayloadJson = d.PayloadJson,
        CreatedOn = d.CreatedOn,
        UpdatedOn = d.UpdatedOn
    };
}
