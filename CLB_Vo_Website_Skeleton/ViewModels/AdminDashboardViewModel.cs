using CLBVo.Models;

namespace CLBVo.ViewModels;

public class AdminDashboardViewModel
{
    // Counters
    public int PostCount { get; set; }
    public int MemberCount { get; set; }
    public int EventCount { get; set; }
    public int GalleryCount { get; set; }
    public int AchievementCount { get; set; }

    // Recent data
    public List<Post> RecentPosts { get; set; } = new();
    public List<Member> RecentMembers { get; set; } = new();
    public List<ClubEvent> UpcomingEvents { get; set; } = new();

    // Chart data: số bài viết 6 tháng gần nhất
    public List<string> ChartMonths { get; set; } = new();
    public List<int> ChartPostCounts { get; set; } = new();

    // Admin info
    public string AdminName { get; set; } = "Admin";
    public string CurrentDate { get; set; } = "";

    public int RegistrationCount { get; set; }
public int NewRegistrationCount { get; set; }
}