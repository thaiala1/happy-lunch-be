using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HappyLunchBE.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Route("api/CartController")]
[Authorize(Roles = "User,Admin")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly HappylunchContext _context;

    public CartController(HappylunchContext context)
    {
        _context = context;
    }

    // Lấy giỏ hàng của người dùng
    [HttpGet("GetCart")]
    public async Task<IActionResult> GetCart()
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

        if (cart == null)
        {
            return Ok(new
            {
                success = true,
                message = "Giỏ hàng trống.",
                data = new { items = new object[0], total = 0 }
            });
        }

        var response = new
        {
            cart.Id,
            Items = cart.CartItems.Select(i => new
            {
                i.Id,
                i.ProductId,
                i.Product.Name,
                i.Quantity,
                UnitPrice = i.Price,
                TotalPrice = i.Price * i.Quantity,
                i.IsSelected
            }),
            TotalSelected = cart.CartItems.Where(i => i.IsSelected).Sum(i => i.Price * i.Quantity)
        };

        return Ok(new { success = true, message = "Lấy giỏ hàng thành công", data = response });
    }

    // Thêm sản phẩm vào giỏ hàng
    [HttpPost("AddItems")]
    public async Task<IActionResult> AddToCart(AddToCartRequest req)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var cart = await _context.Carts.FirstOrDefaultAsync(x => x.UserId == userId && x.Status == "Active");

        if (cart == null)
        {
            cart = new Cart { UserId = userId, Status = "Active" };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        var product = await _context.Products.FindAsync(req.ProductId);
        if (product == null) return BadRequest(new { success = false, message = "Không tìm thấy sản phẩm." });

        var price = product.SalePrice ?? product.RegularPrice;

        var existingItem = await _context.CartItems.FirstOrDefaultAsync(x => x.CartId == cart.Id && x.ProductId == req.ProductId);

        if (existingItem == null)
        {
            var item = new CartItem
            {
                CartId = cart.Id,
                ProductId = req.ProductId,
                Quantity = req.Quantity,
                Price = price
            };

            _context.CartItems.Add(item);
        }
        else
        {
            existingItem.Quantity += req.Quantity;
            existingItem.UpdatedAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Thêm sản phẩm vào giỏ hàng thành công" });
    }

    // Cập nhật số lượng sản phẩm trong giỏ hàng
    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> UpdateQuantity(long itemId, UpdateQuantityRequest req)
    {
        var item = await _context.CartItems.FindAsync(itemId);
        if (item == null) return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });

        item.Quantity = req.Quantity;
        item.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Cập nhật số lượng thành công", data = item });
    }

    // Xóa sản phẩm khỏi giỏ hàng
    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> DeleteItem(long itemId)
    {
        var item = await _context.CartItems.FindAsync(itemId);
        if (item == null) return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Xóa sản phẩm thành công" });
    }

    // Xóa toàn bộ giỏ hàng
    [HttpDelete("{cartId}")]
    public async Task<IActionResult> ClearCart(long cartId)
    {
        var items = _context.CartItems.Where(x => x.CartId == cartId);
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Xóa toàn bộ giỏ hàng thành công" });
    }

    // Chọn hoặc bỏ chọn sản phẩm trong giỏ hàng
    [HttpPatch("{itemId}")]
public async Task<IActionResult> SelectItem(long itemId, SelectItemRequest req)
{
    var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    var item = await _context.CartItems
        .Include(x => x.Cart)
        .FirstOrDefaultAsync(x => x.Id == itemId && x.Cart.UserId == userId);

    if (item == null)
        return NotFound(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng." });

    item.IsSelected = req.IsSelected;
    item.UpdatedAt = DateTime.Now;

    await _context.SaveChangesAsync();

    // Tính lại tổng tiền sản phẩm được chọn
    var totalSelected = await _context.CartItems
        .Where(ci => ci.CartId == item.CartId && ci.IsSelected)
        .SumAsync(ci => ci.Price * ci.Quantity);

    return Ok(new
    {
        success = true,
        message = "Cập nhật trạng thái sản phẩm thành công",
        data = new
        {
            item.Id,
            item.ProductId,
            item.Quantity,
            item.Price,
            item.IsSelected,
            TotalSelected = totalSelected
        }
    });
    }
}

public class AddToCartRequest
{
    public long ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateQuantityRequest
{
    public int Quantity { get; set; }
}

public class SelectItemRequest
{
    public bool IsSelected { get; set; }
}
