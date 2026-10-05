using CLBVo.Data;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/gallery")]
public class AdminGalleryController : Controller
{
    private readonly AppDbContext _db;
    private readonly CloudinaryService _cloudinary;

    public AdminGalleryController(AppDbContext db, CloudinaryService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var images = await _db.GalleryImages.OrderByDescending(g => g.CreatedAt).ToListAsync();
        return View(images);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new GalleryImage());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, List<IFormFile> imageFiles)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError("", "Vui lòng nhập tiêu đề.");
            return View(new GalleryImage());
        }
        if (imageFiles == null || imageFiles.Count == 0 || imageFiles.All(f => f.Length == 0))
        {
            ModelState.AddModelError("", "Vui lòng chọn ít nhất 1 ảnh.");
            return View(new GalleryImage());
        }

        try
        {
            var count = 0;
            foreach (var file in imageFiles.Where(f => f.Length > 0))
            {
                var url = await _cloudinary.UploadAsync(file, "clb-vo/gallery");
                if (string.IsNullOrEmpty(url)) continue;

                _db.GalleryImages.Add(new GalleryImage
                {
                    Title = title,
                    ImageUrl = url,
                    CreatedAt = DateTime.Now
                });
                count++;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã thêm {count} ảnh vào thư viện.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(new GalleryImage());
        }
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var img = await _db.GalleryImages.FindAsync(id);
        if (img == null) return NotFound();
        return View(img);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GalleryImage img, IFormFile? imageFile)
    {
        if (id != img.Id) return NotFound();
        if (!ModelState.IsValid) return View(img);

        var existing = await _db.GalleryImages.FindAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.Title = img.Title;

            if (imageFile != null && imageFile.Length > 0)
            {
                await _cloudinary.DeleteAsync(existing.ImageUrl);
                var newUrl = await _cloudinary.UploadAsync(imageFile, "clb-vo/gallery");
                if (!string.IsNullOrEmpty(newUrl))
                    existing.ImageUrl = newUrl;
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật ảnh.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(img);
        }
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var img = await _db.GalleryImages.FindAsync(id);
        if (img == null) return NotFound();

        await _cloudinary.DeleteAsync(img.ImageUrl);
        _db.GalleryImages.Remove(img);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa ảnh.";
        return RedirectToAction(nameof(Index));
    }
}