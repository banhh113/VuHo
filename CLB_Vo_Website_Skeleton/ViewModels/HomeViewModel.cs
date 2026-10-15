using CLBVo.Models;

namespace CLBVo.ViewModels;

public class HomeViewModel
{
    public List<Post> Posts { get; set; } = new();
    public List<Member> BoxingMembers { get; set; } = new();
    public List<Member> KickboxingMembers { get; set; } = new();
    public List<Member> Coaches { get; set; } = new();

    // ⭐ Mới: Trợ giảng
    public List<Member> AssistantCoaches { get; set; } = new();

    public List<ClubEvent> Events { get; set; } = new();
    public List<GalleryImage> Gallery { get; set; } = new();

    public int GoldCount { get; set; }
    public int SilverCount { get; set; }
    public int BronzeCount { get; set; }
    public int TournamentCount { get; set; }

    public List<TournamentResult> TopAchievements { get; set; } = new();

    public ClubInfo? ClubInfo { get; set; }

    public List<TimeSlot> TimeSlots { get; set; } = new();
    public List<ClubLocation> ClubLocations { get; set; } = new();
}