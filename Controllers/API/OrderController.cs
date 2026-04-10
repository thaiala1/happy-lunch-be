using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HappyLunchBE.Models;
using HappyLunchBE.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace HappyLunchBE.Controllers
{
    [ApiController]
    [Route("api/OrderController")]
    public class OrdersController : ControllerBase
    {
        private readonly HappylunchContext _context;

        public OrdersController(HappylunchContext context)
        {
            _context = context;
        }
        
        /// Tạo đơn hàng mới từ giỏ hàng
        [HttpPost("CreateOrder")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult CreateOrder([FromBody] CreateOrderDto dto)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            var userId = long.Parse(userClaim.Value);


            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId && c.Status == "Active");

            if (cart == null || !cart.CartItems.Any())
                return BadRequest("Giỏ hàng trống");

            // Filter only selected items (IsSelected = true)
            var selectedCartItems = cart.CartItems.Where(ci => ci.IsSelected).ToList();
            
            if (!selectedCartItems.Any())
                return BadRequest("Vui lòng chọn ít nhất một sản phẩm để thanh toán");

            // Calculate total amount only from selected items
            var totalAmount = selectedCartItems.Sum(i => i.Quantity * i.Price);

            var order = new Order
            {
                UserId = userId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                DeliveryBranch = dto.DeliveryBranch,
                DeliveryTimeCode = dto.DeliveryTimeCode,
                Note = dto.Note,
                PaymentMethod = dto.PaymentMethod,
                TotalAmount = (long)totalAmount,
                CreatedAt = DateTime.Now
            };

            // COD
            if (dto.PaymentMethod == "COD")
            {
                order.Status = "PENDING";
                order.OrderStatus = "PENDING_CONFIRMATION";
            }
            // VNPay (chỉ tạo order, thanh toán xử lý ở API khác)
            else if (dto.PaymentMethod == "VNPAY")
            {
                order.Status = "PENDING";
                order.OrderStatus = "PENDING_CONFIRMATION";
            }
            else
            {
                return BadRequest("PaymentMethod không hợp lệ");
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Create OrderItems only from selected cart items
            foreach (var cartItem in selectedCartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price
                };
                _context.OrderItems.Add(orderItem);
            }
            
            _context.SaveChanges();

            return Ok(new
            {
                message = "Đặt hàng thành công",
                orderId = order.Id,
                paymentMethod = order.PaymentMethod
            });
        }

        // Lấy danh sách đơn hàng của người dùng hiện tại
        [HttpGet("MyOrders")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetMyOrders()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            var userId = long.Parse(userClaim.Value);

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.Id,
                    o.TotalAmount,
                    o.Status,
                    o.OrderStatus,
                    o.PaymentMethod,
                    o.CreatedAt
                })
                .ToList();

            return Ok(orders);
        }
        // Lấy chi tiết đơn hàng của user
        [HttpGet("{orderId}")]
        [Authorize(Roles = "User,Admin")]
        public IActionResult GetOrderDetail(long orderId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            var userId = long.Parse(userClaim.Value);

            var order = _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefault(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });

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
    
    return Ok(new
    {
        success = true,
        data = orderDetail
    });
} 
    }
}
namespace HappyLunchBE.DTOs
{
    public class CreateOrderDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string DeliveryBranch { get; set; } = null!;
        public string DeliveryTimeCode { get; set; } = null!;
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = null!; 
    }
}


