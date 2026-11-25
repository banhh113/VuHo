using System.Text;
using System.Text.RegularExpressions;

namespace CLBVo.Helpers;

public static class MedalHelper
{
    public static string GetMedalType(string? medal)
    {
        if (string.IsNullOrWhiteSpace(medal)) return "other";

        // Normalize về FormC và chuyển thành chữ thường
        var normalized = medal.Normalize(NormalizationForm.FormC).ToLowerInvariant();
        
        // Thay thế ký tự không phải chữ cái, dấu (combining marks) hoặc khoảng trắng bằng khoảng trắng
        var clean = Regex.Replace(normalized, @"[^\p{L}\p{M}\s]", " ");
        
        // Tách thành các từ nguyên vẹn
        var words = clean.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Contains("vàng") || words.Contains("nhất")) return "gold";
        if (words.Contains("bạc") || words.Contains("nhì")) return "silver";
        if (words.Contains("đồng") || words.Contains("ba")) return "bronze";
        
        return "other";
    }
}