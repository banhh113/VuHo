using System.ComponentModel.DataAnnotations;

namespace CLBVo.Models;

public class SocialLink
{
    public int Id { get; set; }

    /// <summary>
    /// Nền tảng: facebook | zalo | youtube | tiktok | instagram
    /// </summary>
    [Required(ErrorMessage = "Vui lòng chọn nền tảng")]
    [MaxLength(50)]
    public string Platform { get; set; } = "facebook";

    [Required(ErrorMessage = "Vui lòng nhập URL")]
    [MaxLength(500)]
    public string Url { get; set; } = "";

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}