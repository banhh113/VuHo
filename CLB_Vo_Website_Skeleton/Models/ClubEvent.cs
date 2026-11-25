using CLBVo.Helpers;

namespace CLBVo.Models;

public class ClubEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime EventDate { get; set; } = TimeHelper.NowVN();
    public string Location { get; set; } = "";
    public string? ImageUrl { get; set; }
}