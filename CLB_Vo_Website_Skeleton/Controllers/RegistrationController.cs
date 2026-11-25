using CLBVo.Data;
using CLBVo.Helpers;
using CLBVo.Models;
using CLBVo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace CLBVo.Controllers;

[Route("dang-ky")]
public class RegistrationController : Controller
{
    private readonly AppDbContext _db;

    public RegistrationController(AppDbContext db) => _db = db;

    private const string CaptchaAnswerKey = "RegCaptchaAnswer";
    private const string CaptchaTimeKey = "RegCaptchaTime";

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.TimeSlots = await _db.TimeSlots
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Id)
            .ToListAsync();

        GenerateCaptcha();
        return View(new RegistrationInputModel());
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("RegistrationPolicy")] 
    public async Task<IActionResult> Index(RegistrationInputModel model, string? captchaAnswer, string? website)
    {
        if (!string.IsNullOrWhiteSpace(website))
            ModelState.AddModelError("", "Yêu cầu không hợp lệ. Vui lòng thử lại.");

        var timeStr = HttpContext.Session.GetString(CaptchaTimeKey);
        if (!string.IsNullOrEmpty(timeStr) &&
            DateTime.TryParse(timeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var issuedAt))
        {
            var elapsed = (DateTime.UtcNow - issuedAt.ToUniversalTime()).TotalSeconds;
            if (elapsed < 1) ModelState.AddModelError("", "Vui lòng đợi vài giây rồi gửi lại.");
        }

        var expectedAnswer = HttpContext.Session.GetString(CaptchaAnswerKey);
        var userAnswer = captchaAnswer?.Trim() ?? "";

        if (string.IsNullOrEmpty(expectedAnswer))
            ModelState.AddModelError("", "Phiên làm việc đã hết hạn. Vui lòng tải lại trang.");
        else if (!string.Equals(userAnswer, expectedAnswer, StringComparison.Ordinal))
            ModelState.AddModelError("", "Kết quả phép tính không đúng. Vui lòng thử lại.");

        if (!ModelState.IsValid)
        {
            ViewBag.TimeSlots = await _db.TimeSlots
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .ThenBy(t => t.Id)
                .ToListAsync();
            GenerateCaptcha();
            return View(model);
        }

        // ⭐ Chuẩn hóa Phone (bỏ khoảng trắng)
        var cleanPhone = model.Phone?.Replace(" ", "").Trim() ?? "";

        var reg = new Registration
        {
            FullName = model.FullName,
            Phone = cleanPhone,
            Email = model.Email,
            Age = model.Age,
            PreferredTime = model.PreferredTime,
            Experience = model.Experience,
            Message = model.Message,
            CreatedAt = TimeHelper.NowVN(),
            Status = RegistrationStatus.New,
            RegistrationCode = GenerateRegistrationCode()
        };

        _db.Registrations.Add(reg);
        await _db.SaveChangesAsync();

        HttpContext.Session.Remove(CaptchaAnswerKey);
        HttpContext.Session.Remove(CaptchaTimeKey);

        return RedirectToAction(nameof(ThankYou), new { code = reg.RegistrationCode });
    }

    [HttpGet("captcha")]
    public IActionResult RefreshCaptcha()
    {
        GenerateCaptcha();
        return Json(new { expr = ViewBag.CaptchaExpression });
    }

    [HttpGet("cam-on")]
    public IActionResult ThankYou(string? code)
    {
        ViewBag.RegistrationCode = code;
        return View();
    }

    [HttpGet("tra-cuu")]
    public IActionResult Lookup()
    {
        return View();
    }

    [HttpPost("tra-cuu")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("RegistrationPolicy")] 
    public async Task<IActionResult> Lookup(string phone, string code)
    {
        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(code))
        {
            ViewBag.Error = "Vui lòng nhập đầy đủ Số điện thoại và Mã đăng ký.";
            return View();
        }

        var cleanPhone = phone.Trim().Replace(" ", "");
        var cleanCode = code.Trim().ToUpper();

        // ⭐ So khớp với r.Phone.Replace(" ", "") để tra được cả dữ liệu cũ có khoảng trắng
        var reg = await _db.Registrations
            .FirstOrDefaultAsync(r => r.Phone.Replace(" ", "") == cleanPhone && r.RegistrationCode == cleanCode);

        if (reg == null)
        {
            ViewBag.Error = "Không tìm thấy thông tin đăng ký phù hợp. Vui lòng kiểm tra lại SĐT và Mã.";
            return View();
        }

        return View("LookupResult", reg);
    }

    private void GenerateCaptcha()
    {
        var rng = Random.Shared;
        var a = rng.Next(1, 11);
        var b = rng.Next(1, 11);
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
        HttpContext.Session.SetString(CaptchaTimeKey, DateTime.UtcNow.ToString("o")); // Giữ nguyên UtcNow cho captcha
        ViewBag.CaptchaExpression = expr;
    }

    private string GenerateRegistrationCode()
    {
        var randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"VH-{randomPart}";
    }
}