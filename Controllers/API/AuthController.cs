using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HappyLunchBE.Models;
using HappyLunchBE.Services;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Google.Apis.Auth;

[Route("api/AuthController")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly HappylunchContext _context;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret = "THIS_IS_MY_SUPER_SUPPER_SECRET_KEY_2025_ABCXYZ123456";

    public AuthController(HappylunchContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    // Đăng ký
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
    if (dto.Password != dto.ConfirmPassword)
        return BadRequest(new { success = false, message = "Mật khẩu xác nhận không khớp." });

    if (await _context.Users.AnyAsync(u => u.UserLogin == dto.UserLogin))
        return BadRequest(new { success = false, message = "Người dùng đã tồn tại." });

    if (await _context.Users.AnyAsync(u => u.UserEmail == dto.UserEmail))
        return BadRequest(new { success = false, message = "Email đã được sử dụng." });  

    var code = new Random().Next(100000, 999999).ToString();

    var model = new User
    {
        UserLogin = dto.UserLogin,
        UserEmail = dto.UserEmail,
        UserPass = BCrypt.Net.BCrypt.HashPassword(dto.Password), 
        UserRegistered = DateTime.Now,
        UserStatus = (int)UserStatusEnum.Inactive,
        EmailConfirmationCode = code,
        EmailConfirmed = false,
        Role = "User"
    };

    _context.Users.Add(model);
    await _context.SaveChangesAsync();

    try
    {
        EmailService.SendConfirmationEmail(model.UserEmail, code);
    }
    catch (Exception)
    {
        // Email service may fail, but registration should succeed
        // Return code in response for testing purposes
    }

    return Ok(new { success = true, message = "Vui lòng kiểm tra email để xác nhận tài khoản.", confirmationCode = code });
    }

    
    // Xác nhận email
    [HttpPost("VerifyEmail")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { success = false, message = "Email và mã xác nhận là bắt buộc." });
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == request.Email);
        if (user == null)
            return BadRequest(new { success = false, message = "Không tìm thấy người dùng." });

        if (user.EmailConfirmed == true)
            return BadRequest(new { success = false, message = "Email đã được xác nhận trước đó." });

        if (user.EmailConfirmationCode != request.Code)
            return BadRequest(new { success = false, message = "Không đúng mã xác nhận." });

        user.EmailConfirmed = true;
        user.UserStatus = (int)UserStatusEnum.Active;
        user.EmailConfirmationCode = null;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Đăng ký thành công." });
    }
    
    // Đăng nhập
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin == dto.UserLogin);
        if (user == null)
            return BadRequest(new { success = false, message = "Người dùng không tồn tại." });

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.UserPass))
            return BadRequest(new { success = false, message = "Mật khẩu không đúng." });

        if (user.UserStatus != (int)UserStatusEnum.Active)
            return BadRequest(new { success = false, message = "Tài khoản chưa được kích hoạt." });

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            success = true,
            message = "Đăng nhập thành công",
            token,
            user = new
            {
                user.Id,
                user.DisplayName,
                user.UserEmail,
                user.Role
            }
        });
    }
    
    // Exchange Google authorization code for ID token
    [HttpPost("ExchangeGoogleCode")]
    public async Task<IActionResult> ExchangeGoogleCode([FromBody] GoogleCodeExchangeDto dto)
    {
        if (string.IsNullOrEmpty(dto.Code) || string.IsNullOrEmpty(dto.RedirectUri))
            return BadRequest(new { success = false, message = "Thiếu code hoặc redirect URI." });

        try
        {
            var clientId = _configuration["GoogleOAuth:ClientId"] ?? "29734699934-etss5vc7bibpsmeg2sn4c3qj0v89mo6d.apps.googleusercontent.com";
            var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
            
            if (string.IsNullOrEmpty(clientSecret) || clientSecret == "YOUR_CLIENT_SECRET_HERE")
            {
                return BadRequest(new { success = false, message = "Google Client Secret chưa được cấu hình. Vui lòng thêm vào appsettings.json" });
            }

            // Exchange authorization code for tokens
            using var httpClient = new HttpClient();
            var requestBody = new Dictionary<string, string>
            {
                { "code", dto.Code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", dto.RedirectUri },
                { "grant_type", "authorization_code" }
            };

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(requestBody));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return BadRequest(new { success = false, message = "Không thể exchange code: " + errorContent });
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>();
            
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.IdToken))
            {
                return BadRequest(new { success = false, message = "Không nhận được ID token từ Google." });
            }

            return Ok(new { success = true, idToken = tokenResponse.IdToken });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = "Lỗi khi exchange code: " + ex.Message });
        }
    }

    // Đăng nhập với Google
    [HttpPost("LoginWithGoogle")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginDto dto)
    {
        if (string.IsNullOrEmpty(dto.IdToken))
            return BadRequest(new { success = false, message = "Thiếu Google ID Token." });

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var clientId = _configuration["GoogleOAuth:ClientId"] ?? "29734699934-etss5vc7bibpsmeg2sn4c3qj0v89mo6d.apps.googleusercontent.com";
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { clientId }
            });
        }
        catch
        {
            return BadRequest(new { success = false, message = "ID Token không hợp lệ." });
        }

        var email = payload.Email;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserEmail == email);

        if (user == null)
        {
            user = new User
            {
                UserLogin = email,
                UserEmail = email,
                UserPass = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), 
                DisplayName = payload.Name ?? email,
                UserStatus = (int)UserStatusEnum.Active,
                EmailConfirmed = true,
                UserRegistered = DateTime.Now,
                Role = "User"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            success = true,
            message = "Đăng nhập Google thành công.",
            token,
            user = new
            {
                user.Id,
                user.UserLogin,
                user.UserEmail,
                user.DisplayName,
                user.Role
            }
        });
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserLogin),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = creds,
            Issuer = "yourapp",
            Audience = "yourapp"
        };

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityTokenHandler().CreateToken(tokenDescriptor));
    }
}

public class RegisterDto
{
    public string UserLogin { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
}

public class VerifyEmailRequest 
{ 
    public string Email { get; set; } = null!; 
    public string Code { get; set; } = null!; 
}
public class LoginDto 
{ 
    public string UserLogin { get; set; } = null!; 
    public string Password { get; set; } = null!; 
}
public class GoogleLoginDto 
{ 
    public string IdToken { get; set; } = null!; 
}

public class GoogleCodeExchangeDto
{
    public string Code { get; set; } = null!;
    public string RedirectUri { get; set; } = null!;
}

public class GoogleTokenResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }
    
    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
}
