using CLBVo.Data;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/events")]
public class AdminEventsController : Controller
{
    private readonly AppDbContext _db;
    private readonly CloudinaryService _cloudinary;

    public AdminEventsController(AppDbContext db, CloudinaryService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var events = await _db.Events.OrderByDescending(e => e.EventDate).ToListAsync();
        return View(events);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new ClubEvent { EventDate = DateTime.Now.AddDays(7) });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClubEvent ev, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(ev);
        try
        {
            ev.ImageUrl = await _cloudinary.UploadAsync(imageFile, "clb-vo/events");
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm sự kiện mới.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(ev);
        }
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClubEvent ev, IFormFile? imageFile)
    {
        if (id != ev.Id) return NotFound();
        if (!ModelState.IsValid) return View(ev);

        var existing = await _db.Events.FindAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.Title = ev.Title;
            existing.EventDate = ev.EventDate;
            existing.Location = ev.Location;
            existing.Description = ev.Description;

            if (imageFile != null && imageFile.Length > 0)
            {
                await _cloudinary.DeleteAsync(existing.ImageUrl);
                var newUrl = await _cloudinary.UploadAsync(imageFile, "clb-vo/events");
                if (!string.IsNullOrEmpty(newUrl))
                    existing.ImageUrl = newUrl;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật sự kiện.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(ev);
        }
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null) return NotFound();

        await _cloudinary.DeleteAsync(ev.ImageUrl);
        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa sự kiện.";
        return RedirectToAction(nameof(Index));
    }
}