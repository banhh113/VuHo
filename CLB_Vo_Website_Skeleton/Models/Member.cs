using System.ComponentModel.DataAnnotations;

namespace CLBVo.Models;

public class Member
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    
    [RegularExpression("^(Huấn luyện viên|Võ sinh)$",
        ErrorMessage = "Vai trò chỉ có thể là 'Huấn luyện viên' hoặc 'Võ sinh'")]
    public string Role { get; set; } = "";
    
    public string? AvatarUrl { get; set; }
    public string? Description { get; set; } = "";
}