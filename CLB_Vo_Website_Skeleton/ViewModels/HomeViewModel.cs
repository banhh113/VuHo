using CLBVo.Models;

namespace CLBVo.ViewModels;

public class HomeViewModel
{
    public List<Post> Posts { get; set; } = new();
    public List<Member> Members { get; set; } = new();
    public List<ClubEvent> Events { get; set; } = new();
}