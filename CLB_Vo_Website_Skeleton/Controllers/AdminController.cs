using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin")]
public class AdminController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View();
}