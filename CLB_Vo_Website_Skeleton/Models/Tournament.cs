using System.ComponentModel.DataAnnotations;
using CLBVo.Helpers;

namespace CLBVo.Models;

public class Tournament
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên giải đấu")]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập năm")]
    [Range(1950, 2100, ErrorMessage = "Năm phải từ 1950 đến 2100")]
    public int Year { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = TimeHelper.NowVN();

    public List<TournamentResult> Results { get; set; } = new();
}