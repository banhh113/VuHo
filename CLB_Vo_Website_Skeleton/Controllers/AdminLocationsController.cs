using CLBVo.Data;
using CLBVo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/locations")]
public class AdminLocationsController : Controller
{
    private readonly AppDbContext _db;
    public AdminLocationsController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.ClubLocations
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Id)
            .ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new ClubLocation());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClubLocation model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Name = model.Name ?? "";
        model.Address = model.Address ?? "";

        _db.ClubLocations.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm cơ sở.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var loc = await _db.ClubLocations.FindAsync(id);
        if (loc == null) return NotFound();
        return View(loc);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClubLocation model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var loc = await _db.ClubLocations.FindAsync(id);
        if (loc == null) return NotFound();

        loc.Name = model.Name ?? "";
        loc.Address = model.Address ?? "";
        loc.MapUrl = string.IsNullOrWhiteSpace(model.MapUrl) ? null : model.MapUrl.Trim();
        loc.SortOrder = model.SortOrder;
        loc.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật cơ sở.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var loc = await _db.ClubLocations.FindAsync(id);
        if (loc == null) return NotFound();

        _db.ClubLocations.Remove(loc);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa cơ sở.";
        return RedirectToAction(nameof(Index));
    }
}