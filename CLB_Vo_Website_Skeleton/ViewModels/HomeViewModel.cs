using CLBVo.Models;

namespace CLBVo.ViewModels;

public class HomeViewModel
{
    public List<Post> Posts { get; set; } = new();
    public List<Member> Members { get; set; } = new();
    public List<ClubEvent> Events { get; set; } = new();

    // 4 số đếm cho phần "Dấu ấn của CLB"
    public int GoldCount { get; set; }
    public int SilverCount { get; set; }
    public int BronzeCount { get; set; }
    public int TournamentCount { get; set; }

    // Thông tin CLB (cho phần About / Footer)
    public ClubInfo? ClubInfo { get; set; }
}