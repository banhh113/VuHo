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
        var allMembers = await _db.Members.OrderBy(m => m.Id).ToListAsync();

        var boxingMembers = allMembers.Where(m => m.Category == "Boxing").ToList();
        var kickboxingMembers = allMembers.Where(m => m.Category == "Kickboxing").ToList();
        var coaches = allMembers.Where(m => m.Category == "HLV").ToList();

        var allResults = await _db.TournamentResults.ToListAsync();

        var goldCount = allResults.Count(r =>
            !string.IsNullOrEmpty(r.Medal) &&
            (r.Medal.Contains("vàng", StringComparison.OrdinalIgnoreCase)
             || r.Medal.Contains("nhất", StringComparison.OrdinalIgnoreCase)
             || r.Medal.Contains("🥇")));

        var silverCount = allResults.Count(r =>
            !string.IsNullOrEmpty(r.Medal) &&
            (r.Medal.Contains("bạc", StringComparison.OrdinalIgnoreCase)
             || r.Medal.Contains("nhì", StringComparison.OrdinalIgnoreCase)
             || r.Medal.Contains("🥈")));

        var bronzeCount = allResults.Count(r =>
            !string.IsNullOrEmpty(r.Medal) &&
            (r.Medal.Contains("đồng", StringComparison.OrdinalIgnoreCase)
             || r.Medal.Contains("ba", StringComparison.OrdinalIgnoreCase)
             || r.Medal.Contains("🥉")));

        if (!allResults.Any())
        {
            var achievements = await _db.Achievements.ToListAsync();
            goldCount = achievements.Count(a => !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("vàng", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("nhất", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("🥇")));
            silverCount = achievements.Count(a => !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("bạc", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("nhì", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("🥈")));
            bronzeCount = achievements.Count(a => !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("đồng", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("ba", StringComparison.OrdinalIgnoreCase)
                 || a.Medal.Contains("🥉")));
        }

        var tournamentCount = await _db.Tournaments.CountAsync();

        var topAchievements = await _db.TournamentResults
            .Include(r => r.Tournament)
            .Where(r => !string.IsNullOrEmpty(r.Medal))
            .OrderByDescending(r => r.Tournament!.Year)
            .ThenByDescending(r => r.Id)
            .Take(6)
            .ToListAsync();

        var vm = new HomeViewModel
        {
            Posts = await _db.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync(),
            BoxingMembers = boxingMembers,
            KickboxingMembers = kickboxingMembers,
            Coaches = coaches,
            Events = await _db.Events.Where(e => e.EventDate >= DateTime.Now).OrderBy(e => e.EventDate).ToListAsync(),
            Gallery = await _db.GalleryImages.OrderByDescending(g => g.CreatedAt).ToListAsync(),

            GoldCount = goldCount,
            SilverCount = silverCount,
            BronzeCount = bronzeCount,
            TournamentCount = tournamentCount,

            TopAchievements = topAchievements,

            ClubInfo = await _db.ClubInfos.FirstOrDefaultAsync(),

            // ⭐ Lịch tập + cơ sở động từ admin
            TimeSlots = await _db.TimeSlots
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Id)
                .ToListAsync(),

            ClubLocations = await _db.ClubLocations
                .Where(l => l.IsActive)
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.Id)
                .ToListAsync()
        };

        return View(vm);
    }

    [HttpGet("/news/{id:int}")]
    public async Task<IActionResult> PostDetail(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        return View(post);
    }

    [HttpGet("/events/{id:int}")]
    public async Task<IActionResult> EventDetail(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpGet("/members/{id:int}")]
public async Task<IActionResult> MemberDetail(int id)
{
    var member = await _db.Members.FindAsync(id);
    if (member == null) return NotFound();

    ViewBag.Achievements = await _db.TournamentResults
        .Include(r => r.Tournament)
        .Where(r => r.MemberId == id)
        .OrderByDescending(r => r.Tournament!.Year)
        .ThenByDescending(r => r.Id)
        .ToListAsync();

    return View(member);
}

    [HttpGet("/about")]
    public async Task<IActionResult> About()
    {
        var info = await _db.ClubInfos.FirstOrDefaultAsync();
        return View(info);
    }

    [HttpGet("/thanh-tich")]
    public async Task<IActionResult> Tournaments()
    {
        var list = await _db.Tournaments
            .OrderByDescending(t => t.Year)
            .ThenBy(t => t.Name)
            .Include(t => t.Results)
            .ToListAsync();

        var allResults = list.SelectMany(t => t.Results).ToList();

        var goldCount = allResults.Count(r =>
            !string.IsNullOrEmpty(r.Medal) &&
            (r.Medal.Contains("vàng", StringComparison.OrdinalIgnoreCase) ||
             r.Medal.Contains("nhất", StringComparison.OrdinalIgnoreCase) ||
             r.Medal.Contains("🥇")));

        var silverCount = allResults.Count(r =>
            !string.IsNullOrEmpty(r.Medal) &&
            (r.Medal.Contains("bạc", StringComparison.OrdinalIgnoreCase) ||
             r.Medal.Contains("nhì", StringComparison.OrdinalIgnoreCase) ||
             r.Medal.Contains("🥈")));

        var bronzeCount = allResults.Count(r =>
            !string.IsNullOrEmpty(r.Medal) &&
            (r.Medal.Contains("đồng", StringComparison.OrdinalIgnoreCase) ||
             r.Medal.Contains("ba", StringComparison.OrdinalIgnoreCase) ||
             r.Medal.Contains("🥉")));

        if (!list.Any())
        {
            var achievements = await _db.Achievements.ToListAsync();
            goldCount = achievements.Count(a => !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("vàng", StringComparison.OrdinalIgnoreCase) ||
                 a.Medal.Contains("nhất", StringComparison.OrdinalIgnoreCase) ||
                 a.Medal.Contains("🥇")));
            silverCount = achievements.Count(a => !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("bạc", StringComparison.OrdinalIgnoreCase) ||
                 a.Medal.Contains("nhì", StringComparison.OrdinalIgnoreCase) ||
                 a.Medal.Contains("🥈")));
            bronzeCount = achievements.Count(a => !string.IsNullOrEmpty(a.Medal) &&
                (a.Medal.Contains("đồng", StringComparison.OrdinalIgnoreCase) ||
                 a.Medal.Contains("ba", StringComparison.OrdinalIgnoreCase) ||
                 a.Medal.Contains("🥉")));
        }

        var vm = new TournamentsViewModel
        {
            Tournaments = list,
            GoldCount = goldCount,
            SilverCount = silverCount,
            BronzeCount = bronzeCount,
            TotalResults = allResults.Count
        };
        return View(vm);
    }

    [HttpGet("/thanh-tich/{id:int}")]
    public async Task<IActionResult> TournamentDetail(int id)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Results)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (tournament == null) return NotFound();
        return View(tournament);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();
}