using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HappyLunchBE.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace HappyLunchBE.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly HappylunchContext _context;

        public AdminOrdersController(HappylunchContext context)
        {
            _context = context;
        }

        // Lấy toàn bộ đơn hàng
        [HttpGet]
        public IActionResult GetAllOrders()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });
            var userId = long.Parse(userClaim.Value);
            var orders = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    UserName = o.User.DisplayName,
                    o.Phone,
                    o.TotalAmount,
                    o.Status,
                    o.OrderStatus,
                    o.PaymentMethod,
                    o.CreatedAt
                }).ToList();

            return Ok(new { success = true, data = orders });
        }

        // Xem chi tiết 1 đơn hàng
        [HttpGet("{orderId}")]
        public IActionResult GetOrderDetail(long orderId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });
            var userId = long.Parse(userClaim.Value);
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

            var orderDetail = new
            {
                order.Id,
                order.FirstName,
                order.LastName,
                order.Phone,
                order.DeliveryBranch,
                order.DeliveryTimeCode,
                order.Note,
                order.PaymentMethod,
                order.Status,
                order.OrderStatus,
                order.TotalAmount,
                order.CreatedAt,
                UserName = order.User.DisplayName,
                Items = order.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.ProductId,
                    ProductName = oi.Product.Name,
                    oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return Ok(new { success = true, data = orderDetail });
        }

        // Cập nhật OrderStatus và Status
        [HttpPut("{orderId}/update-status")]
        public IActionResult UpdateOrderStatus(long orderId, [FromBody] UpdateOrderStatusDto dto)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });
            var userId = long.Parse(userClaim.Value);
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null)
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

            var validOrderStatus = new[] { "PENDING_CONFIRMATION", "CONFIRMED", "DELIVERING", "DELIVERED", "CANCELED", "REFUNDED" };
            var validStatus = new[] { "Pending", "Paid" };

            if (!validOrderStatus.Contains(dto.OrderStatus))
                return BadRequest(new { success = false, message = "OrderStatus không hợp lệ" });
            if (!validStatus.Contains(dto.Status))
                return BadRequest(new { success = false, message = "Status không hợp lệ" });

            order.OrderStatus = dto.OrderStatus;
            order.Status = dto.Status;
            _context.SaveChanges();

            return Ok(new { success = true, message = "Cập nhật trạng thái thành công" });
        }
    }

    public class UpdateOrderStatusDto
    {
        public string OrderStatus { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
