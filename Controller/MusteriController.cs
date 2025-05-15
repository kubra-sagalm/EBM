using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EBM.Data;
using EBM.Models;

namespace EBM.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "musteri")] // Bu controller sadece müşteri giriş yaptıysa erişilir
public class MusteriController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MusteriController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🎁 Ödül alma işlemi
    [HttpPost("odul-al")]
    public IActionResult OdulAl([FromBody] int odulId)
    {
        var email = User.FindFirstValue("name");
        var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (kullanici == null)
            return Unauthorized("Kullanıcı bulunamadı.");

        var odul = _context.Oduller.FirstOrDefault(o => o.Id == odulId);
        if (odul == null)
            return NotFound("Ödül bulunamadı.");

        if (odul.KullaniciId != null)
            return BadRequest("Bu ödül zaten alınmış.");

        if (kullanici.CipBakiye < odul.GerekliCip)
            return BadRequest("Yeterli cip bakiyeniz yok.");

        // Çipi düş ve ödülü ilişkilendir
        kullanici.CipBakiye -= odul.GerekliCip;
        odul.KullaniciId = kullanici.Id;
        odul.AlinmaTarihi = DateTime.UtcNow; // ✅ UTC time


        _context.SaveChanges();
        return Ok("Ödül başarıyla alındı.");
    }
    
    [HttpGet("odul/listele")]
    public IActionResult TumOdulleriListele()
    {
        // KullaniciId ve AlinmaTarihi boş olan ödüller henüz kimse tarafından alınmamıştır
        var oduller = _context.Oduller
            .Where(o => o.KullaniciId == null && o.AlinmaTarihi == null)
            .ToList();

        return Ok(oduller);
    }
}