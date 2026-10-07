using AuthService.API.Data;
using AuthService.API.DTOs;
using AuthService.API.Models;
using AuthService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(AuthDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var emailNormalized = request.Email.ToLower().Trim();

        // Validar duplicado
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailNormalized))
        {
            return BadRequest(new { message = "El correo electrónico ya se encuentra registrado." });
        }

        var user = new User
        {
            Email = emailNormalized,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role,
            Expiration = DateTime.UtcNow.AddHours(8)
        });
    }

    /// <summary>
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var emailNormalized = request.Email.ToLower().Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalized);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Credenciales incorrectas (correo o contraseña no válidos)." });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { message = "La cuenta de usuario se encuentra inactiva." });
        }

        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role,
            Expiration = DateTime.UtcNow.AddHours(8)
        });
    }

    /// <summary>
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido o expirado." });
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "Usuario no encontrado." });
        }

        return Ok(new
        {
            user.Id,
            user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            user.Role,
            user.IsActive,
            user.CreatedAt
        });
    }
}
