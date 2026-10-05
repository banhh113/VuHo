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
        var allAchievements = await _db.Achievements.ToListAsync();
        var allMembers = await _db.Members.OrderBy(m => m.Id).ToListAsync();

        var coaches = allMembers
            .Where(m => !string.IsNullOrEmpty(m.Role) &&
                (m.Role.Contains("Huấn luyện", StringComparison.OrdinalIgnoreCase)
                 || m.Role.Contains("HLV", StringComparison.OrdinalIgnoreCase)
                 || m.Role.Contains("Coach", StringComparison.OrdinalIgnoreCase)
                 || m.Role.Contains("Trợ giảng", StringComparison.OrdinalIgnoreCase)
                 || m.Role.Contains("Giáo viên", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var students = allMembers.Where(m => !coaches.Contains(m)).ToList();

        var vm = new HomeViewModel
        {
            Posts = await _db.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync(),
            Coaches = coaches,
            Students = students,
            Events = await _db.Events.Where(e => e.EventDate >= DateTime.Now).OrderBy(e => e.EventDate).ToListAsync(),
            Gallery = await _db.GalleryImages.OrderByDescending(g => g.CreatedAt).ToListAsync(),

            GoldCount = allAchievements.Count(a =>
                !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("vàng", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("🥇")
                 || a.Medal.Contains("HCV", StringComparison.OrdinalIgnoreCase))),

            SilverCount = allAchievements.Count(a =>
                !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("bạc", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("🥈")
                 || a.Medal.Contains("HCB", StringComparison.OrdinalIgnoreCase))),

            BronzeCount = allAchievements.Count(a =>
                !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("đồng", StringComparison.OrdinalIgnoreCase)
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

    // ==================== TRANG CHI TIẾT BÀI VIẾT ====================
    [HttpGet("/news/{id:int}")]
    public async Task<IActionResult> PostDetail(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        return View(post);
    }

    // ==================== TRANG CHI TIẾT SỰ KIỆN ====================
    [HttpGet("/events/{id:int}")]
    public async Task<IActionResult> EventDetail(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    // ==================== TRANG CHI TIẾT THÀNH VIÊN ====================
    [HttpGet("/members/{id:int}")]
    public async Task<IActionResult> MemberDetail(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();
        return View(member);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}