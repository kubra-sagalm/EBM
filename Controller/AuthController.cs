using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EBM.Data;
using EBM.Models;

namespace EBM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;

    public AuthController(IConfiguration config, ApplicationDbContext context)
    {
        _config = config;
        _context = context;
    }

    // ✅ Kullanıcı Kaydı
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterModel model)
    {
        // Email zaten kayıtlı mı kontrol et
        if (_context.Kullanicilar.Any(u => u.Email == model.Email))
        {
            return BadRequest("Bu e-posta adresi zaten kayıtlı.");
        }

        var yeniKullanici = new Kullanici
        {
            AdSoyad = model.AdSoyad,
            Email = model.Email,
            Sifre = model.Sifre,
            Telefon = model.Telefon,
            Adres = model.Adres,
            Rol = model.Rol,
            CipBakiye = 0,
            ParaBakiye = 0
        };

        _context.Kullanicilar.Add(yeniKullanici);
        _context.SaveChanges();

        return Ok("Kayıt başarılı.");
    }

    // ✅ Giriş Yap (JWT Token)
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var user = _context.Kullanicilar.FirstOrDefault(u => u.Email == model.Email && u.Sifre == model.Sifre);
        if (user == null)
        {
            return Unauthorized("Geçersiz e-posta veya şifre.");
        }

        var claims = new[]
        {
            new Claim("name", user.Email),
            new Claim("role", user.Rol),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}
