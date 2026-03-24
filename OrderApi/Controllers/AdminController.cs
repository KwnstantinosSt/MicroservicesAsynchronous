using Microsoft.AspNetCore.Mvc;

namespace OrderApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("Ping")]
    public IActionResult Ping()
    {
        return Ok($"Pong - {DateTime.Now}");
    }
}