using Microsoft.AspNetCore.Mvc;

namespace CLBVo.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}
