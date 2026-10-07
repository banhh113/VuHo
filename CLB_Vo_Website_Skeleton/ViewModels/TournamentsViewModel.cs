using CLBVo.Models;

namespace CLBVo.ViewModels;

public class TournamentsViewModel
{
    public List<Tournament> Tournaments { get; set; } = new();
    public int GoldCount { get; set; }
    public int SilverCount { get; set; }
    public int BronzeCount { get; set; }
    public int TotalResults { get; set; }
}