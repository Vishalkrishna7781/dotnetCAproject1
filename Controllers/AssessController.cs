using Microsoft.AspNetCore.Mvc;
using Sthairya.Models;
using Sthairya.Services;

namespace Sthairya.Controllers;

/// <summary>JSON endpoint called by dashboard.js. Returns data, not a page. Stores nothing.</summary>
[ApiController]
[Route("api/assess")]
public class AssessController : ControllerBase
{
    private readonly RiskEngine _engine;
    public AssessController(RiskEngine engine) => _engine = engine;

    [HttpPost]
    public IActionResult Post([FromBody] VitalsReading reading)
    {
        var error = _engine.Validate(reading);                 // range checks
        return error is null
            ? Ok(_engine.Assess(reading))                      // 200 + Assessment as JSON
            : BadRequest(new { error });                       // 400 + message
    }
}
