using System.Security.Claims;
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
    
    [HttpPost("kullaniciya-cip-para-yukle")]
    public IActionResult KullaniciyaCipVeParaYukle([FromBody] CipYuklemeModel model)
    {
        var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.Id == model.KullaniciId);

        if (kullanici == null)
            return NotFound("Kullanıcı bulunamadı.");

        kullanici.CipBakiye += model.EklenecekCip;
        kullanici.ParaBakiye = (kullanici.ParaBakiye ?? 0) + model.EklenecekPara;

        _context.SaveChanges();

        return Ok(new
        {
            mesaj = "Çip ve para başarıyla yüklendi.",
            kullanici = kullanici.AdSoyad,
            yeniCipBakiyesi = kullanici.CipBakiye,
            yeniParaBakiyesi = kullanici.ParaBakiye
        });
    }

    
    [Authorize]
    [HttpGet("cipbakiye")]
    public async Task<IActionResult> CipBakiyem()
    {
        var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(kullaniciIdStr, out int musteriId))
            return Unauthorized("Geçersiz kullanıcı kimliği.");

        var musteri = await _context.Kullanicilar
            .FirstOrDefaultAsync(k => k.Id == musteriId);

        if (musteri == null)
            return NotFound(new { message = "Kullanıcı bulunamadı." });

        return Ok(new
        {
            cipBakiye = musteri.CipBakiye,
            adSoyad = musteri.AdSoyad
        });
    }


    
    
    
    
    

    
    
    
    
}