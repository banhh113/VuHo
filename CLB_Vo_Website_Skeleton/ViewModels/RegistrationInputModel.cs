using System.ComponentModel.DataAnnotations;

namespace CLBVo.ViewModels;

public class RegistrationInputModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [MaxLength(150, ErrorMessage = "Họ tên không vượt quá 150 ký tự")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
    // Regex chấp nhận khoảng trắng, sau khi bỏ khoảng trắng sẽ khớp ^(0|\+84)\d{9}$
    [RegularExpression(@"^(0|\+84)(\s*\d){9}$", ErrorMessage = "Số điện thoại không đúng định dạng Việt Nam")]
    public string Phone { get; set; } = "";

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(150, ErrorMessage = "Email không vượt quá 150 ký tự")]
    public string? Email { get; set; }

    [Range(5, 80, ErrorMessage = "Độ tuổi phải từ 5 đến 80")]
    public int? Age { get; set; }

    [MaxLength(100, ErrorMessage = "Khung giờ không vượt quá 100 ký tự")]
    public string? PreferredTime { get; set; }
    
    [MaxLength(500, ErrorMessage = "Kinh nghiệm không vượt quá 500 ký tự")]
    public string? Experience { get; set; }

    [MaxLength(1000, ErrorMessage = "Lời nhắn không vượt quá 1000 ký tự")]
    public string? Message { get; set; }
}