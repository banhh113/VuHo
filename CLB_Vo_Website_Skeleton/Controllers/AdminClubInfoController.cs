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
    private readonly CloudinaryService _cloudinary;

    public AdminClubInfoController(AppDbContext db, CloudinaryService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
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
        info.Phone = model.Phone ?? "";
        info.Email = model.Email ?? "";
        info.OpeningHours = model.OpeningHours ?? "";

        // KHÔNG đụng info.Address — giữ giá trị cũ trong DB
        // (Địa chỉ giờ quản lý ở AdminLocations)

        if (imageFile != null && imageFile.Length > 0)
        {
            await _cloudinary.DeleteAsync(info.ImageUrl);
            var newUrl = await _cloudinary.UploadAsync(imageFile, "clb-vo/club");
            if (!string.IsNullOrEmpty(newUrl))
                info.ImageUrl = newUrl;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã lưu thông tin CLB.";
        return RedirectToAction(nameof(Index));
    }
}