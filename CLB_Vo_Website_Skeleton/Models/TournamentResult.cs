using System.ComponentModel.DataAnnotations;

namespace CLBVo.Models;

public class TournamentResult
{
    public int Id { get; set; }

    // Cho phép null nếu nhập tay tên giải
    public int? TournamentId { get; set; }
    public Tournament? Tournament { get; set; }

    public int? MemberId { get; set; }
    public Member? Member { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên VĐV")]
    [MaxLength(150)]
    public string MemberName { get; set; } = "";

    // Tên giải nhập tay (khi không chọn giải có sẵn)
    [MaxLength(200)]
    public string? TournamentNameText { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn huy chương")]
    [MaxLength(50)]
    public string Medal { get; set; } = "";

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}