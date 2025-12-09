using Atk.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ApplicationDbContext _context; // untuk ambil user object

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

        return Ok(new
        {
            message = "Login berhasil",
            token,
            role = role.ToString(),
            nama,
            divisi,
            userId
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT tidak bisa “logout”, client cukup menghapus token
        return Ok(new { message = "Logout berhasil, silakan hapus token di client." });
    }
}
