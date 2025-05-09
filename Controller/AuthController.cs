using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EBM.DbContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using EBM.DTO;


namespace EBM.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthController(IConfiguration configuration, ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    
    [HttpPost("login-with-role")]
    public async Task<IActionResult> LoginWithRole([FromBody] LoginWithRoleDto dto)
    {
        var user = await _context.Kullanicilar
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.Rol == dto.SelectedRole);

        if (user == null)
            return Unauthorized("Seçilen role sahip kullanıcı bulunamadı.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Sifre))
            return Unauthorized("Şifre hatalı.");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            userId = user.Id,
            role = user.Rol
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var users = await _context.Kullanicilar
            .Where(u => u.Email == model.Email)
            .ToListAsync();

        if (users.Count == 0)
            return Unauthorized("Kullanıcı bulunamadı.");

        // Şifre doğrulaması ilk eşleşen kullanıcı üzerinden yapılır
        var sifreDogruKullanici = users.FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(model.Password, u.Sifre));

        if (sifreDogruKullanici == null)
            return Unauthorized("Şifre hatalı.");

        // Eğer kullanıcıya ait birden fazla rol varsa rolleri döndür
        if (users.Count > 1)
        {
            var roller = users.Select(u => u.Rol).ToList();
            return Ok(new
            {
                message = "Lütfen rol seçiniz.",
                multipleRoles = true,
                roles = roller
            });
        }

        // Tek bir rol varsa direkt token üret
        return Ok(new
        {
            multipleRoles = false,
            role = sifreDogruKullanici.Rol,
            userId = sifreDogruKullanici.Id
        });
    }


    // Login DTO
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
