using CLBVo.Data;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize]
[Route("admin/posts")]
public class AdminPostsController : Controller
{
    private readonly AppDbContext _db;
    private readonly FileUploadService _upload;

    public AdminPostsController(AppDbContext db, FileUploadService upload)
    {
        _db = db;
        _upload = upload;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var posts = await _db.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(posts);
    }

    [HttpGet("create")]
    public IActionResult Create() => View(new Post());

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Post post, IFormFile? imageFile)
    {
        if (!ModelState.IsValid) return View(post);

        try
        {
            post.ImageUrl = await _upload.UploadAsync(imageFile);
            post.CreatedAt = DateTime.Now;
            _db.Posts.Add(post);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(post);
        }
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        return View(post);
    }

    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Post post, IFormFile? imageFile)
    {
        if (id != post.Id) return NotFound();
        if (!ModelState.IsValid) return View(post);

        var existing = await _db.Posts.FindAsync(id);
        if (existing == null) return NotFound();

        try
        {
            existing.Title = post.Title;
            existing.Content = post.Content;

            if (imageFile != null && imageFile.Length > 0)
            {
                _upload.Delete(existing.ImageUrl);
                existing.ImageUrl = await _upload.UploadAsync(imageFile);
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(post);
        }
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();

        _upload.Delete(post.ImageUrl);
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}