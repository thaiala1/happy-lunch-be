using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;
using VNPAY.Models.Exceptions;
using HappyLunchBE.Models;

namespace HappyLunchBE.Controllers
{
    [ApiController]
    [Route("api/VNPayController")]
    public class VNPayController : ControllerBase
    {
        private readonly ILogger<VNPayController> _logger;
        private readonly IVnpayClient _vnpayClient;
        private readonly HappylunchContext _context;

        public VNPayController(ILogger<VNPayController> logger, IVnpayClient vnpayClient, HappylunchContext context)
        {
            _logger = logger;
            _vnpayClient = vnpayClient;
            _context = context;
        }

        /// Tạo URL thanh toán chi tiết từ OrderId
        [HttpGet("create-url/{orderId}")]
        public IActionResult CreateDetailedPaymentUrl(long orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return NotFound("Đơn hàng không tồn tại");

            var money = order.TotalAmount;

            var request = new VnpayPaymentRequest
            {
                Money = money,
                Description = $"ORDER#{order.Id}",
                BankCode = BankCode.ANY,
                Language = DisplayLanguage.Vietnamese,
            };

            var paymentUrlInfo = _vnpayClient.CreatePaymentUrl(request);

            _logger.LogInformation("Detailed Payment URL created for Order {OrderId}: {Url}", order.Id, paymentUrlInfo.Url);

            return Ok(new { paymentUrl = paymentUrlInfo.Url });
        }
        [HttpGet("callback")]
        [HttpGet("/api/vnpay/callback")] // Route alias for VNPay config compatibility
        public IActionResult VNPayCallback()
        {
            // Lấy toàn bộ query string VNPay gửi về
            var query = HttpContext.Request.Query;

            foreach (var key in query.Keys)
            {
                _logger.LogInformation("VNPay callback: {Key} = {Value}", key, query[key]);
            }

            // Lấy OrderId từ vnp_OrderInfo
            var orderInfo = query["vnp_OrderInfo"].ToString();
            long orderId = 0;

            if (!string.IsNullOrEmpty(orderInfo) && orderInfo.Contains("ORDER#"))
            {
                long.TryParse(orderInfo.Split('#')[1], out orderId);
            }

            if (orderId == 0)
            {
                _logger.LogWarning("VNPay callback không có OrderId hợp lệ");
                return BadRequest("OrderId không hợp lệ");
            }

            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                return NotFound("Đơn hàng không tồn tại");

            // Lấy các thông tin khác
            string transactionNo = query["vnp_TransactionNo"].ToString();
            string responseCode = query["vnp_ResponseCode"].ToString();
            string bankCode = query["vnp_BankCode"].ToString();
            decimal amount = 0;
            decimal.TryParse(query["vnp_Amount"].ToString(), out amount);

            // Lưu giao dịch vào DB
            var transaction = new VnpayTransaction
            {
                OrderId = order.Id,
                TransactionNo = transactionNo,
                Amount = amount / 100, // VNPay trả về *100
                ResponseCode = responseCode,
                BankCode = bankCode,
                OrderInfo = orderInfo,
                CreatedAt = DateTime.Now
            };

            _context.VnpayTransactions.Add(transaction);

            // Cập nhật trạng thái đơn hàng nếu thanh toán thành công
            if (responseCode == "00" && order.Status != "Paid")
            {
                order.Status = "Paid";
            }

            _context.SaveChanges();

            // Return HTML page with redirect instructions
            // Since frontend runs from file://, we can't redirect directly
            // We'll show a page with a clickable link to the order detail
            var isSuccess = responseCode == "00";
            var message = isSuccess ? "Thanh toán thành công!" : "Thanh toán thất bại!";
            var icon = isSuccess ? "✓" : "✗";
            
            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Kết quả thanh toán</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 20px;
        }}
        .container {{
            text-align: center;
            background: white;
            padding: 50px 40px;
            border-radius: 15px;
            box-shadow: 0 10px 40px rgba(0,0,0,0.2);
            max-width: 500px;
            width: 100%;
        }}
        .icon {{
            font-size: 64px;
            margin-bottom: 20px;
            line-height: 1;
        }}
        .success-icon {{
            color: #28a745;
        }}
        .error-icon {{
            color: #dc3545;
        }}
        .title {{
            font-size: 28px;
            font-weight: 700;
            margin-bottom: 20px;
            color: #1a1a1a;
        }}
        .success-title {{
            color: #28a745;
        }}
        .error-title {{
            color: #dc3545;
        }}
        .order-info {{
            background: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            margin: 25px 0;
            text-align: left;
        }}
        .order-info p {{
            margin: 10px 0;
            font-size: 16px;
            color: #333;
        }}
        .order-info strong {{
            color: #1a1a1a;
            display: inline-block;
            min-width: 120px;
        }}
        .message {{
            margin: 25px 0;
            font-size: 16px;
            color: #666;
            line-height: 1.6;
        }}
        .btn-link {{
            display: inline-block;
            margin-top: 20px;
            padding: 14px 28px;
            background: #007bff;
            color: white;
            text-decoration: none;
            border-radius: 8px;
            font-weight: 600;
            font-size: 16px;
            transition: all 0.3s;
            box-shadow: 0 4px 6px rgba(0,123,255,0.3);
        }}
        .btn-link:hover {{
            background: #0056b3;
            transform: translateY(-2px);
            box-shadow: 0 6px 12px rgba(0,123,255,0.4);
        }}
        .btn-link:active {{
            transform: translateY(0);
        }}
        .instructions {{
            margin-top: 30px;
            padding: 15px;
            background: #fff3cd;
            border-left: 4px solid #ffc107;
            border-radius: 5px;
            font-size: 14px;
            color: #856404;
            text-align: left;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon {(isSuccess ? "success-icon" : "error-icon")}'>{icon}</div>
        <h1 class='title {(isSuccess ? "success-title" : "error-title")}'>{message}</h1>
        
        <div class='order-info'>
            <p><strong>Mã đơn hàng:</strong> #{order.Id}</p>
            <p><strong>Mã giao dịch:</strong> {transactionNo}</p>
            <p><strong>Ngân hàng:</strong> {bankCode}</p>
        </div>
        
        <div class='message'>
            <p>Nhấn vào nút bên dưới để xem chi tiết đơn hàng của bạn.</p>
        </div>
        
        <a href='file:///E:/HappyLunch/PTUD 2/OrderDetail.html?id={order.Id}' class='btn-link' id='order-detail-link'>
            Xem chi tiết đơn hàng #{order.Id}
        </a>

    </div>
    
    <script>
        // Try to open the file:// link
        var link = document.getElementById('order-detail-link');
        if (link) {{
            // Try to open in same window after a short delay
            link.onclick = function(e) {{
                // Allow default behavior (browser will handle file://)
                // If it doesn't work, user can manually open the file
            }};
        }}
    </script>
</body>
</html>";

            return Content(htmlContent, "text/html");
        }
    }
}
