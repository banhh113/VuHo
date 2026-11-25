using CLBVo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize(Roles = "HLV,Member")]
[Route("portal")]
public class MemberPortalController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public MemberPortalController(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        
        // ⭐ Thêm dòng này để fix lỗi CS8602 (Dereference of a possibly null reference)
        if (user == null) return Challenge(); 

        var member = await _db.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);

        if (member == null) 
            return NotFound("Không tìm thấy hồ sơ thành viên liên kết với tài khoản này.");

        ViewBag.IsHLV = User.IsInRole("HLV");
        ViewBag.AccountEmail = user.Email;
        ViewBag.AccountId = user.Id;
        
        return View(member);
    }
}