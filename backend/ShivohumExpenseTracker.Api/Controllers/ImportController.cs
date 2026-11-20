using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShivohumExpenseTracker.Api.Models.Dtos;
using ShivohumExpenseTracker.Api.Services;

namespace ShivohumExpenseTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ImportController(ExcelImportService importService) : ControllerBase
{
    [HttpPost("excel")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<ImportResultDto>> Excel(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Please choose an Excel file." });
        }

        var ext = Path.GetExtension(file.FileName);
        if (!ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) &&
            !ext.Equals(".xls", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only .xlsx Excel files are supported." });
        }

        await using var stream = file.OpenReadStream();
        var result = await importService.ImportAsync(stream, User.GetUserId(), cancellationToken);
        return Ok(result);
    }
}
