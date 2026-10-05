using CLBVo.Data;
using CLBVo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index()
    {
        // Lấy tất cả achievements về bộ nhớ để đếm chính xác (tránh collation)
        var allAchievements = await _db.Achievements.ToListAsync();

        var vm = new HomeViewModel
        {
            Posts = await _db.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync(),

            Members = await _db.Members
                .Take(3)
                .ToListAsync(),

            Events = await _db.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventDate)
                .Take(2)
                .ToListAsync(),

            Gallery = await _db.GalleryImages
                .OrderByDescending(g => g.CreatedAt)
                .Take(6)
                .ToListAsync(),

            GoldCount = allAchievements.Count(a =>
                !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("vàng", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("VÀNG", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("🥇")
                 || a.Medal.Contains("HCV", StringComparison.OrdinalIgnoreCase))),

            SilverCount = allAchievements.Count(a =>
                !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("bạc", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("BẠC", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("🥈")
                 || a.Medal.Contains("HCB", StringComparison.OrdinalIgnoreCase))),

            BronzeCount = allAchievements.Count(a =>
                !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("đồng", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("ĐỒNG", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("🥉")
                 || a.Medal.Contains("HCĐ", StringComparison.OrdinalIgnoreCase))),

            TournamentCount = allAchievements
                .Select(a => a.Competition)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .Count(),

            ClubInfo = await _db.ClubInfos.FirstOrDefaultAsync()
        };

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}