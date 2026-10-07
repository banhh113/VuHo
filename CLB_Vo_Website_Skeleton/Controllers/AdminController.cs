using CLBVo.Data;
using CLBVo.Models;
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
        var now = DateTime.Now;
        var sixMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var posts = await _db.Posts
            .Where(p => p.CreatedAt >= sixMonthsAgo)
            .ToListAsync();

        var months = new List<string>();
        var counts = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            var month = sixMonthsAgo.AddMonths(i);
            months.Add($"T{month.Month}");
            counts.Add(posts.Count(p => p.CreatedAt.Year == month.Year && p.CreatedAt.Month == month.Month));
        }

        var vm = new AdminDashboardViewModel
        {
            PostCount = await _db.Posts.CountAsync(),
            MemberCount = await _db.Members.CountAsync(),
            EventCount = await _db.Events.CountAsync(),
            GalleryCount = await _db.GalleryImages.CountAsync(),
            AchievementCount = await _db.Achievements.CountAsync(),

            RegistrationCount = await _db.Registrations.CountAsync(),
            NewRegistrationCount = await _db.Registrations.CountAsync(r => r.Status == RegistrationStatus.New),
            ConfirmedRegistrationCount = await _db.Registrations.CountAsync(r => r.Status == RegistrationStatus.Confirmed),

            RecentPosts = await _db.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync(),

            RecentMembers = await _db.Members
                .OrderByDescending(m => m.Id)
                .Take(4)
                .ToListAsync(),

            UpcomingEvents = await _db.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToListAsync(),

            ChartMonths = months,
            ChartPostCounts = counts,

            AdminName = User.Identity?.Name?.Split('@')[0] ?? "Admin",
            CurrentDate = DateTime.Now.ToString("dddd, dd/MM/yyyy",
                new System.Globalization.CultureInfo("vi-VN"))
        };

        return View(vm);
    }
}