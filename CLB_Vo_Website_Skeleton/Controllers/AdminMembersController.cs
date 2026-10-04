using CLBVo.Data;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/members")]
public class AdminMembersController : Controller
{
    private readonly AppDbContext _db;
    private readonly FileUploadService _upload;

    public AdminMembersController(AppDbContext db, FileUploadService upload)
    {
        _db = db;
        _upload = upload;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var members = await _db.Members.OrderBy(m => m.Id).ToListAsync();
        return View(members);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new Member());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(member);

        try
        {
            member.AvatarUrl = await _upload.UploadAsync(imageFile, "uploads/members");
            _db.Members.Add(member);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm thành viên mới.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(member);
        }
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();
        return View(member);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Member member, IFormFile? imageFile)
    {
        if (id != member.Id) return NotFound();
        if (!ModelState.IsValid) return View(member);

        var existing = await _db.Members.FindAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.FullName = member.FullName;
            existing.Role = member.Role;
            existing.Description = member.Description;

            if (imageFile != null && imageFile.Length > 0)
            {
                _upload.Delete(existing.AvatarUrl);
                existing.AvatarUrl = await _upload.UploadAsync(imageFile, "uploads/members");
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật thành viên.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(member);
        }
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var member = await _db.Members.FindAsync(id);
        if (member == null) return NotFound();

        _upload.Delete(member.AvatarUrl);
        _db.Members.Remove(member);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa thành viên.";
        return RedirectToAction(nameof(Index));
    }
}