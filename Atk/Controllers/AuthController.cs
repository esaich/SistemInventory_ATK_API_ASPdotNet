using Atk.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ApplicationDbContext _context;

    public AuthController(IAuthService auth, ApplicationDbContext context)
    {
        _auth = auth;
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var (success, message, role, nama, divisi, userId) =
            await _auth.LoginAsync(dto.Username, dto.Password);

        if (!success)
            return Unauthorized(new { message });

        // Ambil user object untuk generate token
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return Unauthorized(new { message = "User tidak ditemukan" });

        // Generate JWT Token
        var token = _auth.GenerateJwtToken(user);

        // ✅ SET COOKIE (HttpOnly & Secure)
        Response.Cookies.Append("AuthToken", token, new CookieOptions
        {
            HttpOnly = true,        // Tidak bisa diakses JavaScript (XSS protection)
            Secure = true,          // Hanya HTTPS (set false untuk development)
            SameSite = SameSiteMode.Strict, // CSRF protection
            Expires = DateTimeOffset.UtcNow.AddHours(6), // Sama dengan JWT expiry
            Path = "/"              // Cookie berlaku untuk semua path
        });

        return Ok(new
        {
            message = "Login berhasil",
            // token, // ❌ JANGAN kirim token di response body lagi (sudah di cookie)
            role = role.ToString(),
            nama,
            divisi,
            userId
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // ✅ HAPUS COOKIE
        Response.Cookies.Delete("AuthToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });

        return Ok(new { message = "Logout berhasil, cookie telah dihapus." });
    }

    // ✅ ENDPOINT UNTUK CEK STATUS LOGIN (opsional tapi berguna)
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst("username")?.Value;
        var nama = User.FindFirst("nama")?.Value;
        var divisi = User.FindFirst("divisi")?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new
        {
            userId,
            username,
            nama,
            divisi,
            role
        });
    }
}