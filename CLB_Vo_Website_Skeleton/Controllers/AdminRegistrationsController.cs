using CLBVo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/registrations")]
public class AdminRegistrationsController : Controller
{
    private readonly AppDbContext _db;
    public AdminRegistrationsController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? filter)
    {
        var query = _db.Registrations.AsQueryable();

        if (filter == "new")
            query = query.Where(r => !r.IsContacted);
        else if (filter == "contacted")
            query = query.Where(r => r.IsContacted);

        var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

        ViewBag.Filter = filter;
        ViewBag.TotalCount = await _db.Registrations.CountAsync();
        ViewBag.NewCount = await _db.Registrations.CountAsync(r => !r.IsContacted);

        return View(list);
    }

    [HttpPost("toggle/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();

        reg.IsContacted = !reg.IsContacted;
        await _db.SaveChangesAsync();

        TempData["Success"] = reg.IsContacted ? "Đã đánh dấu liên hệ." : "Đã bỏ đánh dấu.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("note/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNote(int id, string? adminNote)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();

        reg.AdminNote = adminNote ?? "";
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã lưu ghi chú.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();

        _db.Registrations.Remove(reg);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa đăng ký.";
        return RedirectToAction(nameof(Index));
    }
}