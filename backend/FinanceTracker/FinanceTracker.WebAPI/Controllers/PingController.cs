using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class PingController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping() => Ok("Pong");
}