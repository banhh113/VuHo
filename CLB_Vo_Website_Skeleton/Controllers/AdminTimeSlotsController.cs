using CLBVo.Data;
using CLBVo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/time-slots")]
public class AdminTimeSlotsController : Controller
{
    private readonly AppDbContext _db;
    public AdminTimeSlotsController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.TimeSlots
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Id)
            .ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new TimeSlot());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TimeSlot model)
    {
        if (!ModelState.IsValid) return View(model);
        model.Name = model.Name ?? "";
        _db.TimeSlots.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm khung giờ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var slot = await _db.TimeSlots.FindAsync(id);
        if (slot == null) return NotFound();
        return View(slot);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TimeSlot model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var slot = await _db.TimeSlots.FindAsync(id);
        if (slot == null) return NotFound();

        slot.Name = model.Name ?? "";
        slot.SortOrder = model.SortOrder;
        slot.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật khung giờ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var slot = await _db.TimeSlots.FindAsync(id);
        if (slot == null) return NotFound();
        _db.TimeSlots.Remove(slot);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa khung giờ.";
        return RedirectToAction(nameof(Index));
    }
}