namespace CLBVo.Models;

public class TimeSlot
{
    public int Id { get; set; }
    public string Name { get; set; } = "";        // VD: "Sáng (6h - 9h)"
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}