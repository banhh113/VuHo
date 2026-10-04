using CLBVo.Data;
using CLBVo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/achievements")]
public class AdminAchievementsController : Controller
{
    private readonly AppDbContext _db;

    public AdminAchievementsController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Achievements
            .OrderByDescending(a => a.Year)
            .ThenBy(a => a.Title)
            .ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new Achievement { Year = DateTime.Now.Year });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Achievement achievement)
    {
        if (!ModelState.IsValid) return View(achievement);

        _db.Achievements.Add(achievement);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm thành tích.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var achievement = await _db.Achievements.FindAsync(id);
        if (achievement == null) return NotFound();
        return View(achievement);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Achievement achievement)
    {
        if (id != achievement.Id) return NotFound();
        if (!ModelState.IsValid) return View(achievement);

        _db.Achievements.Update(achievement);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật thành tích.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var achievement = await _db.Achievements.FindAsync(id);
        if (achievement == null) return NotFound();

        _db.Achievements.Remove(achievement);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa thành tích.";
        return RedirectToAction(nameof(Index));
    }
}