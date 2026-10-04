using CLBVo.Data;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/club-info")]
public class AdminClubInfoController : Controller
{
    private readonly AppDbContext _db;
    private readonly FileUploadService _upload;

    public AdminClubInfoController(AppDbContext db, FileUploadService upload)
    {
        _db = db;
        _upload = upload;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var info = await _db.ClubInfos.FirstOrDefaultAsync();

        if (info == null)
        {
            info = new ClubInfo();
            _db.ClubInfos.Add(info);
            await _db.SaveChangesAsync();
        }

        return View(info);
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ClubInfo model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(model);

        var info = await _db.ClubInfos.FindAsync(model.Id);
        if (info == null) return NotFound();

        info.Name = model.Name ?? "";
        info.Description = model.Description ?? "";
        info.Address = model.Address ?? "";
        info.Phone = model.Phone ?? "";
        info.Email = model.Email ?? "";
        info.OpeningHours = model.OpeningHours ?? "";   // ← thêm dòng này

        // Upload ảnh mới nếu có
        if (imageFile != null && imageFile.Length > 0)
        {
            _upload.Delete(info.ImageUrl);
            var newUrl = await _upload.UploadAsync(imageFile, "uploads/club");
            if (!string.IsNullOrEmpty(newUrl))
            {
                info.ImageUrl = newUrl;
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã lưu thông tin CLB.";
        return RedirectToAction(nameof(Index));
    }
}