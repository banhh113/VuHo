namespace CLBVo.Models;

public class ClubLocation
{
    public int Id { get; set; }
    public string Name { get; set; } = "";        // VD: "Cơ sở 1 - Tam Phước"
    public string Address { get; set; } = "";     // Địa chỉ đầy đủ
    public string? MapUrl { get; set; }           // Link Google Maps
    public int SortOrder { get; set; } = 0;       // Thứ tự hiển thị
    public bool IsActive { get; set; } = true;    // Bật/tắt
}