using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Tracker.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(HealthCheckService healthService) : ControllerBase
{
    [HttpGet("live")]
    public ActionResult LiveAsync() =>
        Ok(new { status = "live" });

    [HttpGet("ready")]
    public async Task<ActionResult> ReadyAsync(CancellationToken ct = default)
    {
        var report = await healthService.CheckHealthAsync(ct);
        return report.Status == HealthStatus.Healthy
            ? Ok(new { status = "ready" })
            : StatusCode(503);
    }
}