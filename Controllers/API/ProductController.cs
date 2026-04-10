using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HappyLunchBE.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Route("api/ProductController")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly HappylunchContext _context;

    public ProductController(HappylunchContext context)
    {
        _context = context;
    }

    // Lấy danh sách categories
    [HttpGet("Categories")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new
            {
                c.Id,
                c.Name
            })
            .OrderBy(c => c.Id)
            .ToListAsync();

        return Ok(categories);
    }

    // Lấy danh sách sản phẩm cho người dùng
    [HttpGet("PublicProducts")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetPublicProducts()
    { 
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var products = await _context.Products
            .Select(p => new PublicProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category != null ? p.Category.Name : "Không xác định",
                AverageRating = p.Reviews.Any()
                    ? p.Reviews.Average(r => (double)r.Rating)
                    : 0
            })
            .ToListAsync();

        return Ok(products);
    }

    // Admin lấy danh sách sản phẩm
    [HttpGet("AdminProducts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminProducts()
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var products = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category != null ? p.Category.Name : "Không xác định",
                CategoryId = p.CategoryId,
                RegularPrice = p.RegularPrice,
                SalePrice = p.SalePrice,
                StockQuantity = p.StockQuantity,
                StockStatus = p.StockStatus,
                Description = p.Description,
                PublishedDate = p.PublishedDate,
                Images = p.ProductImages.Select(img => img.ImageUrl).ToList()
            })
            .ToListAsync();

        return Ok(products);
    }

    // Lấy chi tiết sản phẩm cho người dùng
    [HttpGet("{id}/PublicDetail")]
    [Authorize(Roles = "User,Admin")]  
    public async Task<IActionResult> GetPublicProductDetail(long id)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var product = await _context.Products
            .Where(p => p.Id == id)
            .Select(p => new PublicProductDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.Category != null ? p.Category.Name : "Không xác định",
                Price = p.SalePrice ?? p.RegularPrice,
                Description = p.Description,
                Images = p.ProductImages.Select(i => i.ImageUrl).ToList(),
                AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => (double)r.Rating) : 0,
                Reviews = p.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto
                {
                    UserName = r.User.DisplayName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound();

        return Ok(product);
    }
    
    // Lấy chi tiết sản phẩm cho admin
    [HttpGet("{id}/AdminDetail")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminProductDetail(long id)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var product = await _context.Products
            .Where(p => p.Id == id)
            .Select(p => new AdminProductDetailDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.Name : "Không xác định",
                RegularPrice = p.RegularPrice,
                SalePrice = p.SalePrice,
                StockStatus = p.StockStatus,
                StockQuantity = p.StockQuantity,
                PublishedDate = p.PublishedDate,
                Images = p.ProductImages.Select(i => i.ImageUrl).ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    // Tạo sản phẩm mới
    [HttpPost("PostNewProduct")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    {
        try
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            var userId = long.Parse(userClaim.Value);

            // Validate CategoryId exists
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoryExists)
            {
                return BadRequest(new { success = false, message = "Danh mục không tồn tại." });
            }

            // Validate ShopId (userId) exists
            var shopExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!shopExists)
            {
                return BadRequest(new { success = false, message = "Shop không tồn tại." });
            }

            // Validate and normalize StockStatus
            var stockStatus = dto.StockStatus;
            if (string.IsNullOrEmpty(stockStatus))
            {
                stockStatus = "InStock";
            }
            // Normalize: convert "in_stock" to "InStock", "out_of_stock" to "OutOfStock"
            stockStatus = stockStatus switch
            {
                "in_stock" => "InStock",
                "out_of_stock" => "OutOfStock",
                "InStock" => "InStock",
                "OutOfStock" => "OutOfStock",
                _ => "InStock"
            };

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                RegularPrice = dto.RegularPrice,
                SalePrice = dto.SalePrice,
                StockStatus = stockStatus,
                StockQuantity = dto.StockQuantity,
                ShopId = userId, // Set ShopId to current user (admin)
                CreatedAt = DateTime.UtcNow,
                PublishedDate = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Thêm sản phẩm thành công.", productId = product.Id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Lỗi khi tạo sản phẩm: {ex.Message}", error = ex.ToString() });
        }
    }

    // Cập nhật sản phẩm
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct(long id, UpdateProductDto dto)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.CategoryId = dto.CategoryId;
        product.RegularPrice = dto.RegularPrice;
        product.SalePrice = dto.SalePrice;
        product.StockStatus = dto.StockStatus;
        product.StockQuantity = dto.StockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok();
    }

    // Xóa sản phẩm
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(long id)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var product = await _context.Products
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        foreach (var img in product.ProductImages)
        {
            var filePath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                img.ImageUrl.TrimStart('/')
            );

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok("Xóa sản phẩm thành công.");
    }

    // Tải ảnh sản phẩm
    [HttpPost("{id}/images")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadProductImages(
    long id,
    List<IFormFile> files)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

    var product = await _context.Products.FindAsync(id);
    if (product == null)
        return NotFound("Không tìm thấy sản phẩm.");

    var uploadPath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        "uploads",
        "products"
    );

    if (!Directory.Exists(uploadPath))
        Directory.CreateDirectory(uploadPath);

    foreach (var file in files)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var image = new ProductImage
        {
            ProductId = id,
            ImageUrl = $"/uploads/products/{fileName}",
            IsPrimary = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProductImages.Add(image);
    }

    await _context.SaveChangesAsync();
    return Ok("Tải ảnh thành công.");
    }

    // Xóa ảnh sản phẩm
    [HttpDelete("images/{imageId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteImage(long imageId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

    var image = await _context.ProductImages.FindAsync(imageId);
    if (image == null)
        return NotFound();

    var filePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        image.ImageUrl.TrimStart('/')
    );

    if (System.IO.File.Exists(filePath))
        System.IO.File.Delete(filePath);

    _context.ProductImages.Remove(image);
    await _context.SaveChangesAsync();

    return Ok();
    }

    // Đặt ảnh chính cho sản phẩm   
    [HttpPut("images/{imageId}/primary")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetPrimaryImage(long imageId)
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

    var image = await _context.ProductImages
        .Include(i => i.Product)
        .FirstOrDefaultAsync(i => i.Id == imageId);

    if (image == null)
        return NotFound();

    var images = _context.ProductImages
        .Where(i => i.ProductId == image.ProductId);

    foreach (var img in images)
        img.IsPrimary = false;

    image.IsPrimary = true;
    await _context.SaveChangesAsync();

    return Ok();
    }
}
public class PublicProductListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public double AverageRating { get; set; }
}

public class AdminProductListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public double? StockQuantity { get; set; }
    public DateTime PublishedDate { get; set; }
}

public class PublicProductDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public List<string> Images { get; set; } = new();
    public double AverageRating { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
}

public class ReviewDto
{
    public string UserName { get; set; } = null!;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminProductDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public decimal RegularPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string StockStatus { get; set; } = null!;
    public double? StockQuantity { get; set; }
    public DateTime PublishedDate { get; set; }
    public List<string> Images { get; set; } = new();
}

public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public long CategoryId { get; set; }
    public decimal RegularPrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string StockStatus { get; set; } = "in_stock";
    public double? StockQuantity { get; set; }
}

public class UpdateProductDto : CreateProductDto { }

