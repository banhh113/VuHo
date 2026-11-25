using CLBVo.Data;
using CLBVo.Helpers;
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
        var assistantCoaches = allMembers.Where(m => m.Category == "Trợ giảng").ToList();

        var allResults = await _db.TournamentResults.ToListAsync();

        // ⭐ Sử dụng MedalHelper
        var goldCount = allResults.Count(r => MedalHelper.GetMedalType(r.Medal) == "gold");
        var silverCount = allResults.Count(r => MedalHelper.GetMedalType(r.Medal) == "silver");
        var bronzeCount = allResults.Count(r => MedalHelper.GetMedalType(r.Medal) == "bronze");

        if (!allResults.Any())
        {
            var achievements = await _db.Achievements.ToListAsync();
            goldCount = achievements.Count(a => MedalHelper.GetMedalType(a.Medal) == "gold");
            silverCount = achievements.Count(a => MedalHelper.GetMedalType(a.Medal) == "silver");
            bronzeCount = achievements.Count(a => MedalHelper.GetMedalType(a.Medal) == "bronze");
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
            AssistantCoaches = assistantCoaches,
            Events = await _db.Events.Where(e => e.EventDate >= TimeHelper.NowVN()).OrderBy(e => e.EventDate).ToListAsync(),
            Gallery = await _db.GalleryImages.OrderByDescending(g => g.CreatedAt).ToListAsync(),

            GoldCount = goldCount,
            SilverCount = silverCount,
            BronzeCount = bronzeCount,
            TournamentCount = tournamentCount,

            TopAchievements = topAchievements,

            ClubInfo = await _db.ClubInfos.FirstOrDefaultAsync(),

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

    // ==================== API: MEMBER DATA (cho modal) ====================
    [HttpGet("/members/{id:int}/data")]
    public async Task<IActionResult> MemberData(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();

        var achievements = await _db.TournamentResults
            .Include(r => r.Tournament)
            .Where(r => r.MemberId == id)
            .OrderByDescending(r => r.Tournament!.Year)
            .ThenByDescending(r => r.Id)
            .Select(r => new
            {
                r.Id,
                r.Medal,
                r.Category,
                r.Note,
                TournamentName = r.Tournament != null ? r.Tournament.Name : r.TournamentNameText,
                Year = r.Tournament != null ? r.Tournament.Year : (int?)null,
                Location = r.Tournament != null ? r.Tournament.Location : null
            })
            .ToListAsync();

        return Json(new
        {
            member = new
            {
                member.Id,
                member.FullName,
                member.Role,
                member.Category,
                member.Description,
                member.AvatarUrl
            },
            achievements
        });
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

        // ⭐ Sử dụng MedalHelper
        var goldCount = allResults.Count(r => MedalHelper.GetMedalType(r.Medal) == "gold");
        var silverCount = allResults.Count(r => MedalHelper.GetMedalType(r.Medal) == "silver");
        var bronzeCount = allResults.Count(r => MedalHelper.GetMedalType(r.Medal) == "bronze");

        if (!list.Any())
        {
            var achievements = await _db.Achievements.ToListAsync();
            goldCount = achievements.Count(a => MedalHelper.GetMedalType(a.Medal) == "gold");
            silverCount = achievements.Count(a => MedalHelper.GetMedalType(a.Medal) == "silver");
            bronzeCount = achievements.Count(a => MedalHelper.GetMedalType(a.Medal) == "bronze");
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