namespace CLBVo.Models;

public class ClubInfo
{
    public int Id { get; set; }
    public string? Name { get; set; } = "Câu lạc bộ Võ";
    public string? Description { get; set; }
    public string? Address { get; set; }   // không giới hạn
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
    public string? OpeningHours { get; set; }
}