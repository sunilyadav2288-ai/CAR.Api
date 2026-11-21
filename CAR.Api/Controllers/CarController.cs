using CAR.Api.Models;
using CAR.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CAR.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarController : ControllerBase
{
    private readonly ICarOrchestrator _orchestrator;

    public CarController(ICarOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        [FromBody] CarRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _orchestrator.ProcessAsync(
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}