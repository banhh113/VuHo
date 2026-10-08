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

    private const string CaptchaAnswerKey = "RegCaptchaAnswer";
    private const string CaptchaTimeKey = "RegCaptchaTime";

    // ==================== GET: FORM ĐĂNG KÝ ====================
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.TimeSlots = await _db.TimeSlots
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Id)
            .ToListAsync();

        GenerateCaptcha();
        return View(new Registration());
    }

    // ==================== POST: SUBMIT FORM ====================
    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Registration model, string? captchaAnswer, string? website)
    {
        // -------- 1. HONEYPOT CHECK --------
        // Field "website" bị ẩn bằng CSS. Bot tự động fill → chặn
        if (!string.IsNullOrWhiteSpace(website))
        {
            // Không báo lỗi cụ thể (bot sẽ không học được)
            ModelState.AddModelError("", "Yêu cầu không hợp lệ. Vui lòng thử lại.");
        }

        // -------- 2. TIME CHECK --------
// Chỉ chặn nếu submit quá nhanh (< 1 giây)
var timeStr = HttpContext.Session.GetString(CaptchaTimeKey);
if (!string.IsNullOrEmpty(timeStr) &&
    DateTime.TryParse(timeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var issuedAt))
{
    var elapsed = (DateTime.UtcNow - issuedAt.ToUniversalTime()).TotalSeconds;
    if (elapsed < 1)
    {
        ModelState.AddModelError("", "Vui lòng đợi vài giây rồi gửi lại.");
    }
}

        // -------- 3. CAPTCHA CHECK --------
        var expectedAnswer = HttpContext.Session.GetString(CaptchaAnswerKey);
        var userAnswer = captchaAnswer?.Trim() ?? "";

        if (string.IsNullOrEmpty(expectedAnswer))
        {
            ModelState.AddModelError("", "Phiên làm việc đã hết hạn. Vui lòng tải lại trang.");
        }
        else if (!string.Equals(userAnswer, expectedAnswer, StringComparison.Ordinal))
        {
            ModelState.AddModelError("", "Kết quả phép tính không đúng. Vui lòng thử lại.");
        }

        // -------- 4. NẾU CÓ LỖI → TRẢ VỀ FORM --------
        if (!ModelState.IsValid)
        {
            ViewBag.TimeSlots = await _db.TimeSlots
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Id)
                .ToListAsync();

            // Tạo captcha mới (không dùng lại cái cũ)
            GenerateCaptcha();
            return View(model);
        }

        // -------- 5. LƯU VÀO DB --------
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

        // Xoá captcha sau khi submit thành công
        HttpContext.Session.Remove(CaptchaAnswerKey);
        HttpContext.Session.Remove(CaptchaTimeKey);

        return RedirectToAction(nameof(ThankYou));
    }

    // ==================== REFRESH CAPTCHA (AJAX) ====================
    [HttpGet("captcha")]
    public IActionResult RefreshCaptcha()
    {
        GenerateCaptcha();
        return Json(new { expr = ViewBag.CaptchaExpression });
    }

    // ==================== TRANG CẢM ƠN ====================
    [HttpGet("cam-on")]
    public IActionResult ThankYou() => View();

    // ==================== HELPER: TẠO CAPTCHA ====================
    private void GenerateCaptcha()
    {
        var rng = Random.Shared;
        var a = rng.Next(1, 11);  // 1-10
        var b = rng.Next(1, 11);  // 1-10

        // 30% dùng phép trừ (đảm bảo không âm)
        bool useSubtract = rng.Next(0, 10) < 3;
        int answer;
        string expr;

        if (useSubtract && a != b)
        {
            var big = Math.Max(a, b);
            var small = Math.Min(a, b);
            answer = big - small;
            expr = $"{big} − {small}";
        }
        else
        {
            answer = a + b;
            expr = $"{a} + {b}";
        }

        HttpContext.Session.SetString(CaptchaAnswerKey, answer.ToString());
        HttpContext.Session.SetString(CaptchaTimeKey, DateTime.UtcNow.ToString("o"));
        ViewBag.CaptchaExpression = expr;
    }
}