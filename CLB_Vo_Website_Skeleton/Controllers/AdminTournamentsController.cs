using CLBVo.Data;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/tournaments")]
public class AdminTournamentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly CloudinaryService _cloudinary;

    public AdminTournamentsController(AppDbContext db, CloudinaryService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Tournaments
            .OrderByDescending(t => t.Year)
            .ThenBy(t => t.Name)
            .Include(t => t.Results)
            .ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new Tournament { Year = TimeHelper.NowVN().Year });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tournament model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(model);

        // ⭐ VALIDATE NGÀY
        if (model.EndDate.HasValue && model.StartDate.HasValue
            && model.EndDate.Value.Date < model.StartDate.Value.Date)
        {
            ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
            return View(model);
        }

        var exists = await _db.Tournaments.AnyAsync(t => t.Name == model.Name && t.Year == model.Year);
        if (exists)
        {
            ModelState.AddModelError("", $"Giải '{model.Name}' năm {model.Year} đã tồn tại. " +
                "Nếu là giải khác, hãy đổi tên phân biệt (VD: 'Giải Toàn Quốc - Miền Nam').");
            return View(model);
        }

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                model.ImageUrl = await _cloudinary.UploadAsync(imageFile, "clb-vo/tournaments");
            }

            model.CreatedAt = TimeHelper.NowVN();
            _db.Tournaments.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm giải đấu.";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Results)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (tournament == null) return NotFound();

        return View(tournament);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tournament model, IFormFile? imageFile)
    {
        if (id != model.Id) return NotFound();

        // ⭐ VALIDATE NGÀY
        if (model.EndDate.HasValue && model.StartDate.HasValue
            && model.EndDate.Value.Date < model.StartDate.Value.Date)
        {
            ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }

        if (!ModelState.IsValid)
        {
            return View(await _db.Tournaments.Include(t => t.Results).FirstOrDefaultAsync(t => t.Id == id));
        }

        var tournament = await _db.Tournaments.FindAsync(id);
        if (tournament == null) return NotFound();

        try
        {
            tournament.Name = model.Name ?? "";
            tournament.Year = model.Year;
            tournament.Location = model.Location ?? "";
            tournament.StartDate = model.StartDate;
            tournament.EndDate = model.EndDate;
            tournament.Description = model.Description ?? "";

            if (imageFile != null && imageFile.Length > 0)
            {
                await _cloudinary.DeleteAsync(tournament.ImageUrl);
                var newUrl = await _cloudinary.UploadAsync(imageFile, "clb-vo/tournaments");
                if (!string.IsNullOrEmpty(newUrl)) tournament.ImageUrl = newUrl;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật giải đấu.";
            return RedirectToAction(nameof(Edit), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            return RedirectToAction(nameof(Edit), new { id });
        }
    }

    // ==================== THÀNH TÍCH CLB ====================

    [HttpPost("add-result/{tournamentId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddResult(int tournamentId, string? medal, string? category, string? note)
    {
        var tournament = await _db.Tournaments.FindAsync(tournamentId);
        if (tournament == null) return NotFound();

        if (string.IsNullOrWhiteSpace(medal))
        {
            TempData["Error"] = "Vui lòng chọn huy chương / thành tích.";
            return RedirectToAction(nameof(Edit), new { id = tournamentId });
        }

        var result = new TournamentResult
        {
            TournamentId = tournamentId,
            MemberId = null,
            MemberName = "CLB Vũ Hổ",
            Medal = medal,
            Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        };

        _db.TournamentResults.Add(result);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm thành tích CLB.";
        return RedirectToAction(nameof(Edit), new { id = tournamentId });
    }

    [HttpPost("delete-result/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResult(int id)
    {
        var result = await _db.TournamentResults.FindAsync(id);
        if (result == null) return NotFound();

        var tournamentId = result.TournamentId ?? 0;
        _db.TournamentResults.Remove(result);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa thành tích.";
        return RedirectToAction(nameof(Edit), new { id = tournamentId });
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var tournament = await _db.Tournaments
            .Include(t => t.Results)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (tournament == null) return NotFound();

        await _cloudinary.DeleteAsync(tournament.ImageUrl);
        _db.Tournaments.Remove(tournament);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa giải đấu và tất cả thành tích.";
        return RedirectToAction(nameof(Index));
    }
}