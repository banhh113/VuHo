namespace CLBVo.Models;

public class Achievement
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Competition { get; set; } = "";
    public string Medal { get; set; } = "";
    public int Year { get; set; }
}
