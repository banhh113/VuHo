using CLBVo.Data;
using CLBVo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/social-links")]
public class AdminSocialLinksController : Controller
{
    private readonly AppDbContext _db;

    public AdminSocialLinksController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _db.SocialLinks
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();
        return View(list);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new SocialLink { SortOrder = 0 });

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SocialLink model)
    {
        if (!ModelState.IsValid) return View(model);

        model.Platform = model.Platform ?? "facebook";
        model.Url = model.Url ?? "";
        model.DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim();

        _db.SocialLinks.Add(model);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm liên kết MXH.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.SocialLinks.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SocialLink model)
    {
        if (id != model.Id) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var item = await _db.SocialLinks.FindAsync(id);
        if (item == null) return NotFound();

        item.Platform = model.Platform ?? "facebook";
        item.Url = model.Url ?? "";
        item.DisplayName = string.IsNullOrWhiteSpace(model.DisplayName) ? null : model.DisplayName.Trim();
        item.SortOrder = model.SortOrder;
        item.IsActive = model.IsActive;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.SocialLinks.FindAsync(id);
        if (item == null) return NotFound();

        _db.SocialLinks.Remove(item);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xoá.";
        return RedirectToAction(nameof(Index));
    }
}