using CLBVo.Data;
using CLBVo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Route("dang-ky")]
public class RegistrationController : Controller
{
    private readonly AppDbContext _db;

    public RegistrationController(AppDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.TimeSlots = await _db.TimeSlots
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Id)
            .ToListAsync();

        return View(new Registration());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Registration model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.TimeSlots = await _db.TimeSlots
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Id)
                .ToListAsync();
            return View(model);
        }

        model.FullName = model.FullName ?? "";
        model.Phone = model.Phone ?? "";
        model.Email = model.Email ?? "";
        model.Message = model.Message ?? "";
        model.PreferredTime = model.PreferredTime ?? "";
        model.Experience = model.Experience ?? "";
        model.CreatedAt = DateTime.Now;
        model.Status = RegistrationStatus.New;
        
        _db.Registrations.Add(model);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(ThankYou));
    }

    [HttpGet("cam-on")]
    public IActionResult ThankYou() => View();
}