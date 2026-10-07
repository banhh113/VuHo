using CLBVo.Models;

namespace CLBVo.ViewModels;

public class AdminDashboardViewModel
{
    // Số lượng
    public int PostCount { get; set; }
    public int MemberCount { get; set; }
    public int EventCount { get; set; }
    public int GalleryCount { get; set; }
    public int AchievementCount { get; set; }
    public int RegistrationCount { get; set; }
    public int NewRegistrationCount { get; set; }         // ← THÊM
    public int ConfirmedRegistrationCount { get; set; }   // ← THÊM

    // Dữ liệu gần đây
    public List<Post> RecentPosts { get; set; } = new();
    public List<Member> RecentMembers { get; set; } = new();
    public List<ClubEvent> UpcomingEvents { get; set; } = new();

    // Chart
    public List<string> ChartMonths { get; set; } = new();
    public List<int> ChartPostCounts { get; set; } = new();

    // Admin info
    public string AdminName { get; set; } = "Admin";
    public string CurrentDate { get; set; } = "";
}