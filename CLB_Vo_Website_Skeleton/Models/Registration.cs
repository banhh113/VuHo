namespace CLBVo.Models;

public class Registration
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public int? Age { get; set; }
    public string? PreferredTime { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsContacted { get; set; } = false;
    public string? AdminNote { get; set; }
}