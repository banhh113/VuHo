using CLBVo.Data;
using CLBVo.Helpers;
using CLBVo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Authorize(Roles = "Admin,Staff")]
[Route("admin/registrations")]
public class AdminRegistrationsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminRegistrationsController(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? status, string? keyword, DateTime? fromDate, DateTime? toDate)
    {
        var query = _db.Registrations.AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<RegistrationStatus>(status, out var st))
            query = query.Where(r => r.Status == st);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(r => r.FullName.Contains(kw) || r.Phone.Contains(kw) || (r.RegistrationCode != null && r.RegistrationCode.Contains(kw)));
        }

        if (fromDate.HasValue) query = query.Where(r => r.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(r => r.CreatedAt <= endOfDay);
        }

        var list = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

        ViewBag.StatusFilter = status;
        ViewBag.Keyword = keyword;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        ViewBag.CountNew = await _db.Registrations.CountAsync(r => r.Status == RegistrationStatus.New);
        ViewBag.CountContacting = await _db.Registrations.CountAsync(r => r.Status == RegistrationStatus.Contacting);
        ViewBag.CountConfirmed = await _db.Registrations.CountAsync(r => r.Status == RegistrationStatus.Confirmed);
        ViewBag.CountJoined = await _db.Registrations.CountAsync(r => r.Status == RegistrationStatus.Joined);
        ViewBag.CountArchived = await _db.Registrations.CountAsync(r => r.Status == RegistrationStatus.Archived);
        ViewBag.TotalCount = await _db.Registrations.CountAsync();

        return View(list);
    }

    [HttpGet("detail/{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();
        return View(reg);
    }

    [HttpPost("approve-member/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveMember(int id)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();

        string email = string.IsNullOrWhiteSpace(reg.Email) ? $"{reg.Phone}@vuhoclb.vn" : reg.Email.Trim();
        // ⭐ Sử dụng helper sinh mật khẩu ngẫu nhiên
        string defaultPassword = PasswordHelper.GenerateRandomPassword();

        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            TempData["Error"] = $"Tài khoản với email/sđt {email} đã tồn tại trên hệ thống!";
            return RedirectToAction(nameof(Detail), new { id });
        }

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, defaultPassword);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Member");

            var newMember = new Member
            {
                FullName = reg.FullName,
                UserId = user.Id,
                Category = "Boxing",
                Role = "Võ sinh",
                Description = $"Kinh nghiệm: {reg.Experience ?? "Chưa có"}. Lời nhắn: {reg.Message}"
            };
            _db.Members.Add(newMember);

            reg.Status = RegistrationStatus.Joined;
            reg.UpdatedAt = TimeHelper.NowVN();
            // ⭐ Đã bỏ mật khẩu khỏi AdminNote
            reg.AdminNote += $"\n[{TimeHelper.NowVN():dd/MM/yyyy}] Đã tạo tài khoản: {email}";
            
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Thành công! Tài khoản: {email} | Mật khẩu: {defaultPassword}";
        }
        else
        {
            TempData["Error"] = "Lỗi tạo tài khoản: " + string.Join(", ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost("update-status/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, RegistrationStatus status, DateTime? trialDate)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();

        reg.Status = status;
        reg.UpdatedAt = TimeHelper.NowVN();

        if (status == RegistrationStatus.Contacting && !reg.ContactedAt.HasValue) reg.ContactedAt = TimeHelper.NowVN();
        if (status == RegistrationStatus.Confirmed) reg.ConfirmedAt = TimeHelper.NowVN();
        if (trialDate.HasValue) reg.TrialDate = trialDate;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật trạng thái.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost("save-trial-date/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTrialDate(int id, DateTime? trialDate)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();

        reg.TrialDate = trialDate;
        reg.UpdatedAt = TimeHelper.NowVN();

        await _db.SaveChangesAsync();

        TempData["Success"] = trialDate.HasValue
            ? $"Đã lưu ngày tập thử: {trialDate.Value:dd/MM/yyyy}"
            : "Đã xóa ngày tập thử.";

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost("save-note/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNote(int id, string? adminNote, DateTime? trialDate)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();

        reg.AdminNote = adminNote ?? "";
        if (trialDate.HasValue) reg.TrialDate = trialDate;
        reg.UpdatedAt = TimeHelper.NowVN();

        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã lưu ghi chú.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var reg = await _db.Registrations.FindAsync(id);
        if (reg == null) return NotFound();

        _db.Registrations.Remove(reg);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa đăng ký.";
        return RedirectToAction(nameof(Index));
    }
}