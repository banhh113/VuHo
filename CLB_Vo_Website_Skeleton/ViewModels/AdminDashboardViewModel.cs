using CLBVo.Models;

namespace CLBVo.ViewModels;

public class AdminDashboardViewModel
{
    public int PostCount { get; set; }
    public int MemberCount { get; set; }
    public int EventCount { get; set; }
    public int GalleryCount { get; set; }
    public int AchievementCount { get; set; }

    public List<Post> RecentPosts { get; set; } = new();
    public List<ClubEvent> UpcomingEvents { get; set; } = new();
}