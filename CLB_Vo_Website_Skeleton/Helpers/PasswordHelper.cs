using System;
using System.Security.Cryptography;

namespace CLBVo.Helpers;

public static class PasswordHelper
{
    // Tạo mật khẩu ngẫu nhiên >= 12 ký tự, đáp ứng yêu cầu Identity
    public static string GenerateRandomPassword()
    {
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "@#$%^&*!";

        var chars = new char[12];

        // Đảm bảo có đủ các loại ký tự (sử dụng RandomNumberGenerator an toàn)
        chars[0] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
        chars[1] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
        chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        chars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

        string all = lower + upper + digits + special;
        for (int i = 4; i < 12; i++)
        {
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
        }

        // Xáo trộn mảng (Fisher-Yates) bằng RandomNumberGenerator
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }
}