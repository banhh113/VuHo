using CLBVo.Data;
using CLBVo.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CLBVo.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public IActionResult Index()
    {
        var vm = new HomeViewModel
        {
            Posts = _db.Posts.OrderByDescending(p => p.CreatedAt).Take(3).ToList(),
            Members = _db.Members.Take(3).ToList(),
            Events = _db.Events.OrderBy(e => e.EventDate).Take(2).ToList()
        };
        return View(vm);
    }

    public IActionResult Error() => View();
}