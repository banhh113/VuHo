using Microsoft.AspNetCore.Mvc;

namespace CLBVo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIChatController : ControllerBase
{
    private readonly IConfiguration _config;

    public AIChatController(IConfiguration config)
    {
        _config = config;
    }

    public class ChatRequest
    {
        public string Message { get; set; } = "";
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { reply = "Vui lòng nhập câu hỏi." });

        var msg = request.Message.ToLower();

        // ============================================================
        // FALLBACK LOGIC (Trả lời tự động dựa trên từ khóa - FAQ)
        // (Khi có API Key thật, bạn có thể gọi API OpenAI/Gemini ở đây)
        // ============================================================
        
        if (msg.Contains("boxing") || msg.Contains("đấm"))
        {
            return Ok(new { reply = "Boxing (Quyền Anh) tại CLB Vũ Hổ tập trung vào kỹ thuật ra đòn tay, né tránh và di chuyển linh hoạt. Rất tốt cho việc rèn luyện thể lực và phản xạ." });
        }
        if (msg.Contains("kickboxing") || msg.Contains("đá"))
        {
            return Ok(new { reply = "Kickboxing là sự kết hợp giữa đấm bốc và đá. Bộ môn này giúp phát triển toàn diện cơ thể, đặc biệt là sức mạnh của chân và sự dẻo dai." });
        }
        if (msg.Contains("đăng ký") || msg.Contains("tập thử"))
        {
            return Ok(new { reply = "Bạn có thể đăng ký tập thử miễn phí bằng cách bấm vào nút 'Đăng ký tập thử' trên menu, hoặc truy cập đường dẫn /dang-ky. Hãy để lại SĐT, CLB sẽ liên hệ với bạn trong 24h." });
        }
        if (msg.Contains("địa chỉ") || msg.Contains("ở đâu"))
        {
            return Ok(new { reply = "CLB có cơ sở tại Biên Hòa, Đồng Nai. Bạn vui lòng xem chi tiết địa chỉ và bản đồ ở mục 'Liên hệ' trên trang chủ nhé." });
        }
        if (msg.Contains("giờ") || msg.Contains("lịch"))
        {
            return Ok(new { reply = "Lịch tập của CLB được cập nhật thường xuyên. Bạn xem mục 'Lịch tập' trên trang chủ để biết khung giờ chính xác nhé." });
        }
        if (msg.Contains("học phí") || msg.Contains("giá"))
        {
            return Ok(new { reply = "Học phí tại CLB Vũ Hổ rất linh hoạt tùy theo gói tập. Vui lòng gọi hotline 0948 660 111 để được tư vấn chi tiết." });
        }

        // Câu trả lời mặc định
        return Ok(new { reply = "Xin chào! Tôi là trợ lý ảo của CLB Vũ Hổ. Tôi có thể giúp bạn tìm hiểu về các bộ môn Boxing/Kickboxing, địa chỉ, lịch tập hoặc hướng dẫn đăng ký tập thử. Bạn cần hỏi gì ạ?" });
    }
}