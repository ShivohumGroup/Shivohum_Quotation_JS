using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShivohumExpenseTracker.Api.Data;
using ShivohumExpenseTracker.Api.Models.Dtos;
using ShivohumExpenseTracker.Api.Models.Entities;
using ShivohumExpenseTracker.Api.Services;

namespace ShivohumExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> Get()
    {
        var projects = await db.Projects.AsNoTracking().OrderBy(p => p.ProjectName).ToListAsync();
        var received = await ReceivedByProjectAsync();
        var expenses = await ExpenseByProjectAsync();

        var items = projects.Select(project =>
        {
            received.TryGetValue(project.ProjectId, out var paid);
            expenses.TryGetValue(project.ProjectId, out var spent);
            return ProjectFinance.ToDto(project, paid, spent);
        }).ToList();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDto>> GetById(int id)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectId == id);
        if (project is null)
        {
            return NotFound();
        }

        var received = await db.ClientPayments.Where(p => p.ProjectId == id).SumAsync(p => (decimal?)p.Amount) ?? 0;
        var expense = await db.Transactions
            .Where(t => t.ProjectId == id && t.Type == "Expense")
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return Ok(ProjectFinance.ToDto(project, received, expense));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create(ProjectWriteRequest request)
    {
        var error = Validate(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        var name = request.ProjectName.Trim();
        if (await db.Projects.AnyAsync(p => p.ProjectName == name))
        {
            return BadRequest(new { message = "A project with this name already exists." });
        }

        var entity = new Project
        {
            ProjectName = name,
            ClientName = string.IsNullOrWhiteSpace(request.ClientName) ? null : request.ClientName.Trim(),
            Status = NormalizeStatus(request.Status),
            StartDate = request.StartDate?.Date,
            ContractAmount = request.ContractAmount,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            IsActive = true,
            CreatedOn = DateTime.UtcNow
        };

        db.Projects.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.ProjectId }, ProjectFinance.ToDto(entity, 0, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectDto>> Update(int id, ProjectWriteRequest request)
    {
        var entity = await db.Projects.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        var error = Validate(request);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        var name = request.ProjectName.Trim();
        if (await db.Projects.AnyAsync(p => p.ProjectName == name && p.ProjectId != id))
        {
            return BadRequest(new { message = "A project with this name already exists." });
        }

        entity.ProjectName = name;
        entity.ClientName = string.IsNullOrWhiteSpace(request.ClientName) ? null : request.ClientName.Trim();
        entity.Status = NormalizeStatus(request.Status);
        entity.StartDate = request.StartDate?.Date;
        entity.ContractAmount = request.ContractAmount;
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await db.SaveChangesAsync();

        var received = await db.ClientPayments.Where(p => p.ProjectId == id).SumAsync(p => (decimal?)p.Amount) ?? 0;
        var expense = await db.Transactions
            .Where(t => t.ProjectId == id && t.Type == "Expense")
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return Ok(ProjectFinance.ToDto(entity, received, expense));
    }

    private async Task<Dictionary<int, decimal>> ReceivedByProjectAsync() =>
        await db.ClientPayments.AsNoTracking()
            .GroupBy(p => p.ProjectId)
            .Select(g => new { g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Total);

    private async Task<Dictionary<int, decimal>> ExpenseByProjectAsync() =>
        await db.Transactions.AsNoTracking()
            .Where(t => t.Type == "Expense" && t.ProjectId != null)
            .GroupBy(t => t.ProjectId!.Value)
            .Select(g => new { g.Key, Total = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(x => x.Key, x => x.Total);

    private static string? Validate(ProjectWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectName))
        {
            return "Project name is required.";
        }

        if (request.ContractAmount is < 0)
        {
            return "Client budget cannot be negative.";
        }

        return null;
    }

    private static string NormalizeStatus(string status) =>
        status is "Completed" or "OnHold" or "Active" ? status : "Active";
}
