using CLBVo.Models;

namespace CLBVo.ViewModels;

public class HomeViewModel
{
    public List<Post> Posts { get; set; } = new();
    public List<Member> Members { get; set; } = new();
    public List<ClubEvent> Events { get; set; } = new();
    public List<GalleryImage> Gallery { get; set; } = new();   // ← THÊM

    public int GoldCount { get; set; }
    public int SilverCount { get; set; }
    public int BronzeCount { get; set; }
    public int TournamentCount { get; set; }

    public ClubInfo? ClubInfo { get; set; }
}