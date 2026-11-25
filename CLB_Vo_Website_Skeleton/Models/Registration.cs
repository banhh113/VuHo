using CLBVo.Helpers;

namespace CLBVo.Models;

public enum RegistrationStatus
{
    New = 0,           // Mới đăng ký, chưa xử lý
    Contacting = 1,    // Đang liên hệ, chưa chốt
    Confirmed = 2,     // Khách đã xác nhận tham gia, có ngày tập thử
    Joined = 3,        // Đã vào tập chính thức
    Archived = 4,      // Đăng ký ảo / spam / không có nhu cầu
    Cancelled = 5      // Khách hủy
}

public class Registration
{
    public int Id { get; set; }
    public string RegistrationCode { get; set; } = ""; // ⭐ THÊM DÒNG NÀY
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public int? Age { get; set; }
    public string? PreferredTime { get; set; }
    public string? Experience { get; set; }        // Kinh nghiệm tập luyện
    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; } = TimeHelper.NowVN();
    public RegistrationStatus Status { get; set; } = RegistrationStatus.New;

    // Ghi chú & lịch sử liên hệ
    public string? AdminNote { get; set; }
    public DateTime? ContactedAt { get; set; }     // Lần LH gần nhất
    public DateTime? ConfirmedAt { get; set; }     // Ngày xác nhận
    public DateTime? TrialDate { get; set; }       // Ngày tập thử dự kiến
    public DateTime? UpdatedAt { get; set; }       // Lần cập nhật cuối

    // Giữ lại để tương thích ngược (deprecated)
    [Obsolete("Dùng Status thay thế")]
    public bool IsContacted { get; set; } = false;
}