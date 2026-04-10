using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HappyLunchBE.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BCrypt.Net; 

[Route("api/UserController")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly HappylunchContext _context;
    public UserController(HappylunchContext context)
    {
        _context = context;
    }

    // Lấy tất cả người dùng (Admin)
    [HttpGet("GetAllUsers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userClaim == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        var userId = long.Parse(userClaim.Value);

        var users = await _context.Users
        .Select(u => new
        {
            u.Id,
            u.UserLogin,
            u.UserEmail,
            u.DisplayName,
            u.UserStatus,
            u.Role,
        })
        .ToListAsync();
    return Ok(users);
    }

    // Lấy thông tin cá nhân
    [HttpGet("GetOneUser")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetMyProfile()
    {
    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!long.TryParse(userIdString, out var userId))
    {
        return Unauthorized(new { message = "Chưa đăng nhập." });
    }

    var user = await _context.Users
        .Where(u => u.Id == userId)
        .Select(u => new
        {
            u.UserLogin,
            u.UserEmail,
            u.DisplayName
        })
        .FirstOrDefaultAsync();
        return Ok(user);
    }

    // Cập nhật thông tin cá nhân
    [HttpPut("UpdateMyProfile")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> UpdateMyProfile(UpdateMyProfileDto dto)
    {
    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!long.TryParse(userIdString, out var userId))
    {
        return Unauthorized(new { message = "Chưa đăng nhập." });
    }
    var user = await _context.Users.FindAsync(userId);

    if (user == null) return NotFound();

    if (!string.IsNullOrEmpty(dto.UserLogin))
        user.UserLogin = dto.UserLogin;

    if (!string.IsNullOrEmpty(dto.DisplayName))
        user.DisplayName = dto.DisplayName;

    await _context.SaveChangesAsync();
    return Ok("Cập nhật thông tin thành công.");
    }

    // Đổi mật khẩu
    [HttpPut("UpdatePassword")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "Chưa đăng nhập." });

        if (dto.NewPassword != dto.ConfirmPassword)
            return BadRequest(new { message = "Mật khẩu xác nhận không khớp." });

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.UserPass))
            return BadRequest(new { message = "Mật khẩu cũ không đúng." });

        user.UserPass = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    // Cập nhật người dùng (Admin)
    [HttpPut("{userId}/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminUpdateUser(long userId, AdminUpdateUserDto dto)
    {
        
    var user = await _context.Users.FindAsync(userId);
    if (user == null) return NotFound();

    if (dto.UserStatus.HasValue)
        user.UserStatus = dto.UserStatus.Value;

    if (!string.IsNullOrEmpty(dto.Role))
        user.Role = dto.Role;

    await _context.SaveChangesAsync();
    return Ok("Admin cập nhật user thành công.");
    }

    // Xóa người dùng (Admin)
    [HttpDelete("{userId}/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(long userId)
    {
    var user = await _context.Users.FindAsync(userId);

    if (user == null)
    {
        return NotFound(new { message = "Không tìm thấy người dùng." });
    }

    _context.Users.Remove(user);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Xóa người dùng thành công." });
    }
}
public class UpdateMyProfileDto
{
    public string? UserLogin { get; set; }
    public string? DisplayName { get; set; }
}
public class AdminUpdateUserDto
{
    public int? UserStatus { get; set; }
    public string? Role { get; set; } 
}

public class ChangePasswordDto
{
    [Required]
    public string OldPassword { get; set; } = null!;

    [Required]
    public string NewPassword { get; set; } = null!;

    [Required]
    public string ConfirmPassword { get; set; } = null!;
}



