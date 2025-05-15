using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EBM.Data;
using EBM.Models;
using Microsoft.EntityFrameworkCore;

namespace EBM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 🎁 ÖDÜL EKLEME - sadece yetkili kişiler yapar (istersen Authorize(Roles = "admin") da koyabilirsin)
    [HttpPost("odul/ekle")]
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
    
    
    [HttpPost("AçıkArttırma/kapat")]
    public async Task<IActionResult> AcikArtirmaKapat(int acikArtirmaId)
    {
        var acikArtirma = await _context.AcikArtirmalar
            .Include(a => a.Teklifler)
            .FirstOrDefaultAsync(a => a.Id == acikArtirmaId && a.Durum == "aktif");

        if (acikArtirma == null)
            return NotFound("Açık artırma bulunamadı veya zaten kapalı.");

        var kazananTeklif = acikArtirma.Teklifler
            .Where(t => t.Durum == "geçerli")
            .OrderByDescending(t => t.TeklifTutar)
            .FirstOrDefault();

        if (kazananTeklif == null)
            return BadRequest("Geçerli teklif bulunamadı.");

        // Kazanılan fiyatı açık artırmaya yaz
        acikArtirma.KazanilanFiyat = kazananTeklif.TeklifTutar;
        acikArtirma.KazananFirmaId = kazananTeklif.FirmaId; // opsiyonel
        acikArtirma.Durum = "kapalı";

        await _context.SaveChangesAsync();

        return Ok($"Açık artırma kapatıldı. Kazanan teklif: {kazananTeklif.TeklifTutar}");
    }

    
    
    
    
    

    
    
    
    
}