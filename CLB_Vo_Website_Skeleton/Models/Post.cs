using CLBVo.Helpers;

namespace CLBVo.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = TimeHelper.NowVN();
}