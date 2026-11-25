using CLBVo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CLBVo.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly UserManager<IdentityUser> _users;

    public AccountController(SignInManager<IdentityUser> signIn, UserManager<IdentityUser> users)
    {
        _signIn = signIn;
        _users = users;
    }

    [HttpGet("/account/login")]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost("/account/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);

        // ⭐ Bật lockoutOnFailure: true
        var result = await _signIn.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
        
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var user = await _users.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (await _users.IsInRoleAsync(user, "Admin") || await _users.IsInRoleAsync(user, "Staff"))
                    return RedirectToAction("Index", "Admin");
                
                if (await _users.IsInRoleAsync(user, "HLV") || await _users.IsInRoleAsync(user, "Member"))
                    return RedirectToAction("Index", "MemberPortal");
            }
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Tài khoản đã bị khóa do đăng nhập sai quá 5 lần. Vui lòng thử lại sau 10 phút.");
            return View(model);
        }

        ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
        return View(model);
    }

    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/account/change-password")]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost("/account/change-password")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var result = await _users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (result.Succeeded)
        {
            await _signIn.RefreshSignInAsync(user);
            TempData["Success"] = "Đổi mật khẩu thành công!";
            
            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
                return RedirectToAction("Index", "Admin");
            if (User.IsInRole("HLV") || User.IsInRole("Member"))
                return RedirectToAction("Index", "MemberPortal");
                
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return View(model);
    }
}