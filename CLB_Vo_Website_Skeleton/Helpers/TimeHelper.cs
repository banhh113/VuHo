namespace CLBVo.Helpers;

/// <summary>
/// Helper xử lý thời gian — luôn trả về giờ Việt Nam (UTC+7)
/// bất kể server đang chạy timezone nào (Render = UTC).
/// </summary>
public static class TimeHelper
{
    private const int VietnamOffsetHours = 7;

    /// <summary>
    /// Lấy giờ Việt Nam hiện tại. Dùng thay cho DateTime.Now.
    /// </summary>
    public static DateTime NowVN() => DateTime.UtcNow.AddHours(VietnamOffsetHours);

    /// <summary>
    /// Format giờ Việt Nam theo định dạng dd/MM/yyyy HH:mm
    /// </summary>
    public static string FormatVN(DateTime dt, string format = "dd/MM/yyyy HH:mm")
    {
        var vn = DateTime.SpecifyKind(dt, DateTimeKind.Utc).AddHours(VietnamOffsetHours);
        return vn.ToString(format);
    }
}