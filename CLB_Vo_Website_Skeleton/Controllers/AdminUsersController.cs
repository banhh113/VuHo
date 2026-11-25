using CLBVo.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/users")]
public class AdminUsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        
        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var user in users)
        {
            userRoles[user.Id] = await _userManager.GetRolesAsync(user);
        }
        ViewBag.UserRoles = userRoles;

        return View(users);
    }

    [HttpPost("change-role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string userId, string newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // 1. Không cho tự đổi quyền của chính mình
        var currentUserId = _userManager.GetUserId(User);
        if (userId == currentUserId)
        {
            TempData["Error"] = "Bạn không thể tự thay đổi quyền của chính mình.";
            return RedirectToAction(nameof(Index));
        }

        // 2. Chỉ nhận role tồn tại
        if (!await _roleManager.RoleExistsAsync(newRole))
        {
            TempData["Error"] = "Quyền không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        
        // 3. Không cho gỡ Admin cuối cùng
        if (currentRoles.Contains("Admin") && newRole != "Admin")
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1)
            {
                TempData["Error"] = "Không thể gỡ quyền của Admin cuối cùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, newRole);

        TempData["Success"] = $"Đã đổi quyền của {user.Email} thành {newRole}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("reset-password/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        // ⭐ Sử dụng helper sinh mật khẩu ngẫu nhiên
        var defaultPassword = PasswordHelper.GenerateRandomPassword();
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, defaultPassword);

        if (result.Succeeded)
            TempData["Success"] = $"Đã reset mật khẩu cho {user.Email}. Pass mới: {defaultPassword}";
        else
            TempData["Error"] = "Lỗi reset mật khẩu.";

        return RedirectToAction(nameof(Index));
    }
}