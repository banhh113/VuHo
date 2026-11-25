using CLBVo.Data;
using CLBVo.Helpers;
using CLBVo.Models;
using CLBVo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Kiểm tra cấu hình môi trường Production
if (!builder.Environment.IsDevelopment())
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
    var cloudName = builder.Configuration["CloudinarySettings:CloudName"];
    if (string.IsNullOrEmpty(connStr))
        throw new InvalidOperationException("LỖI BẢO MẬT: Chưa cấu hình ConnectionStrings__DefaultConnection trong biến môi trường.");
    if (string.IsNullOrEmpty(cloudName))
        throw new InvalidOperationException("LỖI BẢO MẬT: Chưa cấu hình CloudinarySettings__CloudName trong biến môi trường.");
}

// Cấu hình ForwardedHeaders
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear(); // ⭐ Sửa từ KnownNetworks thành KnownIPNetworks
    options.KnownProxies.Clear();
});

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

// ⭐ RATE LIMITING (Phân vùng theo IP)
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("RegistrationPolicy", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            })
    );

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Bạn thao tác quá nhanh. Vui lòng thử lại sau 1 phút.", token);
    };
});

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<FileUploadService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedAccount = false;

    // ⭐ LOCKOUT: Sai 5 lần khóa 10 phút
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
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

// ⭐ Gọi UseForwardedHeaders NGAY ĐẦU pipeline
app.UseForwardedHeaders();

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

app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==================== SEED ROLES & ADMIN ====================
try
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        string[] roleNames = { "Admin", "Staff", "HLV", "Member" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        // 2. Tạo tài khoản Admin mặc định
        var adminEmail = "admin@vuhoclb.vn";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var adminPass = builder.Configuration["Seed:AdminPassword"];
            bool isRandomPass = false;
            
            if (string.IsNullOrEmpty(adminPass))
            {
                adminPass = PasswordHelper.GenerateRandomPassword();
                isRandomPass = true;
            }

            var res = await userManager.CreateAsync(admin, adminPass);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                if (isRandomPass)
                    Console.WriteLine($"[INFO] Đã tạo Admin với mật khẩu ngẫu nhiên: {adminPass}");
            }
        }
        else
        {
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // 3. Tạo tài khoản Staff mẫu
        var staffEmail = "staff@vuhoclb.vn";
        var staff = await userManager.FindByEmailAsync(staffEmail);
        if (staff == null)
        {
            staff = new IdentityUser { UserName = staffEmail, Email = staffEmail, EmailConfirmed = true };
            var staffPass = builder.Configuration["Seed:StaffPassword"];
            bool isRandomPass = false;
            
            if (string.IsNullOrEmpty(staffPass))
            {
                staffPass = PasswordHelper.GenerateRandomPassword();
                isRandomPass = true;
            }

            var res = await userManager.CreateAsync(staff, staffPass);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(staff, "Staff");
                if (isRandomPass)
                    Console.WriteLine($"[INFO] Đã tạo Staff với mật khẩu ngẫu nhiên: {staffPass}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[WARN] Lỗi khởi tạo Roles/Admin: {ex.Message}");
}

// SEED SOCIAL LINKS ...
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
        }
    }
}
catch { }

app.Run();