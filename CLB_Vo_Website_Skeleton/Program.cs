using CLBVo.Data;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddHttpContextAccessor();

// ⭐ SESSION — dùng cho CAPTCHA
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".CLBVo.Session";
});

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<CloudinaryService>();

builder.Services.AddScoped<FileUploadService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/login";
});

var app = builder.Build();

app.Use(async (ctx, next) =>
{
    if (string.Equals(ctx.Request.Path.Value, "/health", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.StatusCode = 200;
        await ctx.Response.WriteAsync("{\"status\":\"ok\"}");
        return;
    }
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ⭐ Session phải nằm giữa Routing và Authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==================== SEED ADMIN ====================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var adminEmail = "admin@vuhoclb.vn";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "admin123");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[WARN] Không thể tạo admin lúc khởi động: {ex.Message}");
}

// ==================== SEED SOCIAL LINKS ====================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!db.SocialLinks.Any())
        {
            db.SocialLinks.AddRange(
                new SocialLink { Platform = "facebook", Url = "https://facebook.com", DisplayName = "Facebook CLB Vũ Hổ", SortOrder = 1, IsActive = true },
                new SocialLink { Platform = "zalo", Url = "https://zalo.me/0948660111", DisplayName = "Zalo CLB Vũ Hổ", SortOrder = 2, IsActive = true },
                new SocialLink { Platform = "tiktok", Url = "https://tiktok.com", DisplayName = "TikTok CLB Vũ Hổ", SortOrder = 3, IsActive = true }
            );
            db.SaveChanges();
            Console.WriteLine("[INFO] Đã seed 3 SocialLinks mặc định.");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[WARN] Seed SocialLinks lỗi: {ex.Message}");
}

app.Run();