using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EBM.Data;
using EBM.Models;

namespace EBM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OdulController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OdulController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🎁 ÖDÜL EKLEME - sadece yetkili kişiler yapar (istersen Authorize(Roles = "admin") da koyabilirsin)
    [HttpPost("ekle")]
    // [Authorize(Roles = "admin")] // Eğer sadece admin ekleyecekse bu satırı aktif et
    public IActionResult OdulEkle([FromBody] OdulEkleModel model)
    {
        var yeniOdul = new Odul
        {
            Ad = model.Ad,
            GerekliCip = model.GerekliCip,
        };

        _context.Oduller.Add(yeniOdul);
        _context.SaveChanges();

        return Ok("Ödül başarıyla eklendi.");
    }

    [HttpGet("listele")]
    public IActionResult TumOdulleriListele()
    {
        // KullaniciId ve AlinmaTarihi boş olan ödüller henüz kimse tarafından alınmamıştır
        var oduller = _context.Oduller
            .Where(o => o.KullaniciId == null && o.AlinmaTarihi == null)
            .ToList();

        return Ok(oduller);
    }

    
    
    
    
}