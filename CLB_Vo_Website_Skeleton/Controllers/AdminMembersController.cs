using CLBVo.Data;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/members")]
public class AdminMembersController : Controller
{
    private readonly AppDbContext _db;
    private readonly CloudinaryService _cloudinary;

    public AdminMembersController(AppDbContext db, CloudinaryService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? category)
    {
        var query = _db.Members.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(m => m.Category == category);
        }

        var members = await query.OrderBy(m => m.Id).ToListAsync();

        ViewBag.CategoryFilter = category;
        ViewBag.CountAll = await _db.Members.CountAsync();
        ViewBag.CountBoxing = await _db.Members.CountAsync(m => m.Category == "Boxing");
        ViewBag.CountKickboxing = await _db.Members.CountAsync(m => m.Category == "Kickboxing");
        ViewBag.CountHLV = await _db.Members.CountAsync(m => m.Category == "HLV");

        return View(members);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new Member());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(member);

        try
        {
            member.FullName = member.FullName ?? "";
            member.Role = member.Role ?? "";
            member.Description = member.Description ?? "";
            member.Category = string.IsNullOrEmpty(member.Category) ? "Boxing" : member.Category;

            member.AvatarUrl = await _cloudinary.UploadAsync(imageFile, "clb-vo/members");
            _db.Members.Add(member);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm thành viên mới.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            return View(member);
        }
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();

        ViewBag.Achievements = await _db.TournamentResults
            .Include(r => r.Tournament)
            .Where(r => r.MemberId == id)
            .OrderByDescending(r => r.Tournament!.Year)
            .ThenByDescending(r => r.Id)
            .ToListAsync();

        ViewBag.Tournaments = await _db.Tournaments
            .OrderByDescending(t => t.Year)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return View(member);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Member member, IFormFile? imageFile)
    {
        if (id != member.Id) return NotFound();
        if (!ModelState.IsValid) return View(member);

        var existing = await _db.Members.FindAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.FullName = member.FullName ?? "";
            existing.Role = member.Role ?? "";
            existing.Description = member.Description ?? "";
            existing.Category = string.IsNullOrEmpty(member.Category) ? "Boxing" : member.Category;

            if (imageFile != null && imageFile.Length > 0)
            {
                await _cloudinary.DeleteAsync(existing.AvatarUrl);
                var newUrl = await _cloudinary.UploadAsync(imageFile, "clb-vo/members");
                if (!string.IsNullOrEmpty(newUrl))
                    existing.AvatarUrl = newUrl;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật thành viên.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            return View(member);
        }
    }

    [HttpPost("add-achievement/{memberId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAchievement(int memberId, int? tournamentId, string? tournamentNameText, string medal, string? category, string? note)
    {
        var member = await _db.Members.FindAsync(memberId);
        if (member == null) return NotFound();

        if (string.IsNullOrWhiteSpace(medal))
        {
            TempData["Error"] = "Vui lòng chọn huy chương.";
            return RedirectToAction(nameof(Edit), new { id = memberId });
        }

        if (!tournamentId.HasValue && string.IsNullOrWhiteSpace(tournamentNameText))
        {
            TempData["Error"] = "Vui lòng chọn giải đấu hoặc nhập tên giải.";
            return RedirectToAction(nameof(Edit), new { id = memberId });
        }

        var result = new TournamentResult
        {
            MemberId = memberId,
            MemberName = member.FullName,
            TournamentId = tournamentId,
            TournamentNameText = string.IsNullOrWhiteSpace(tournamentNameText) ? null : tournamentNameText.Trim(),
            Medal = medal ?? "",
            Category = category ?? "",
            Note = note ?? ""
        };

        if (tournamentId.HasValue)
        {
            result.TournamentNameText = null;
        }

        _db.TournamentResults.Add(result);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm thành tích.";
        return RedirectToAction(nameof(Edit), new { id = memberId });
    }

    [HttpPost("delete-achievement/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAchievement(int id)
    {
        var result = await _db.TournamentResults.FindAsync(id);
        if (result == null) return NotFound();

        var memberId = result.MemberId ?? 0;
        _db.TournamentResults.Remove(result);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa thành tích.";
        return RedirectToAction(nameof(Edit), new { id = memberId });
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();

        await _cloudinary.DeleteAsync(member.AvatarUrl);

        var achievements = await _db.TournamentResults.Where(r => r.MemberId == id).ToListAsync();
        _db.TournamentResults.RemoveRange(achievements);

        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa thành viên và thành tích liên quan.";
        return RedirectToAction(nameof(Index));
    }
}