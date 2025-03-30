using Microsoft.AspNetCore.Mvc;

namespace Integration_System.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("hello world");
    }
}
