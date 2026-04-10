using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HappyLunchBE.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace HappyLunchBE.Controllers
{
    [Route("api/ReviewController")]
    [ApiController]
    [Authorize(Roles = "User,Admin")]
    public class ReviewController : ControllerBase
    {
        private readonly HappylunchContext _context;

        public ReviewController(HappylunchContext context)
        {
            _context = context;
        }

        // Lấy danh sách review của 1 sản phẩm
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetReviews(long productId)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

            var userId = long.Parse(userClaim.Value);

            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.UserId,
                    UserName = r.User.DisplayName,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách review thành công",
                data = reviews
            });
        }

        // Tạo review mới
        [HttpPost("{productId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateReview(long productId, [FromBody] CreateReviewDto dto)
        {
            
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", data = ModelState });

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out var userId))
                return Unauthorized(new { success = false, message = "Không thể xác định người dùng" });

            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { success = false, message = "Rating phải từ 1 đến 5" });

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Thêm review thành công",
                data = new { reviewId = review.Id }
            });
        }

        // Cập nhật review
        [HttpPut("{reviewId}")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateReview(long reviewId, [FromBody] UpdateReviewDto dto)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return NotFound(new { success = false, message = "Không tìm thấy review" });

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out var userId))
                return Unauthorized(new { success = false, message = "Không thể xác định người dùng" });

            var isAdmin = User.IsInRole("Admin");

            if (review.UserId != userId && !isAdmin)
                return Forbid("Không có quyền cập nhật review này");

            if (dto.Rating.HasValue)
            {
                if (dto.Rating < 1 || dto.Rating > 5)
                    return BadRequest(new { success = false, message = "Rating phải từ 1 đến 5" });
                review.Rating = dto.Rating.Value;
            }

            if (!string.IsNullOrEmpty(dto.Comment)) review.Comment = dto.Comment;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Cập nhật review thành công" });
        }

        // Xóa review
        [HttpDelete("{reviewId}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteReview(long reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                return NotFound(new { success = false, message = "Không tìm thấy review" });

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out var userId))
                return Unauthorized(new { success = false, message = "Không thể xác định người dùng" });

            var isAdmin = User.IsInRole("Admin");

            if (review.UserId != userId && !isAdmin)
                return Forbid("Không có quyền xóa review này");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Xóa review thành công" });
        }
    }

    public class CreateReviewDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }

    public class UpdateReviewDto
    {
        [Range(1, 5, ErrorMessage = "Rating phải từ 1 đến 5")]
        public int? Rating { get; set; }

        public string? Comment { get; set; }
    }
}
