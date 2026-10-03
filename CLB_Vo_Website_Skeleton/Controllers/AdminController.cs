using CLBVo.Data;
using CLBVo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var vm = new AdminDashboardViewModel
        {
            PostCount = await _db.Posts.CountAsync(),
            MemberCount = await _db.Members.CountAsync(),
            EventCount = await _db.Events.CountAsync(),
            GalleryCount = await _db.GalleryImages.CountAsync(),
            AchievementCount = await _db.Achievements.CountAsync(),

            RecentPosts = await _db.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync(),

            UpcomingEvents = await _db.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync()
        };

        return View(vm);
    }
}