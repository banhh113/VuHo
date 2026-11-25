using System.ComponentModel.DataAnnotations;

namespace CLBVo.Models;

public class Member
{
    public int Id { get; set; }

    // ⭐ THÊM DÒNG NÀY ĐỂ LIÊN KẾT VỚI TÀI KHOẢN ĐĂNG NHẬP
    public string? UserId { get; set; } 

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [MaxLength(150)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng chọn vai trò")]
    [MaxLength(100)]
    public string Role { get; set; } = "";

    [MaxLength(50)]
    public string? Category { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Description { get; set; } = "";
}