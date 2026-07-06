using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("actuator")]
public class ActuatorController : ControllerBase
{
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth() =>
        Ok(new
        {
            status = "UP",
            timestampUtc = DateTime.UtcNow
        });
}
