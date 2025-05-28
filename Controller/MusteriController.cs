using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using EBM.Data;
using EBM.Models;
using Microsoft.EntityFrameworkCore;

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

    [HttpPost("odul-al")]
    public IActionResult OdulAl([FromBody] JsonElement body)
    {
        try
        {
            if (!body.TryGetProperty("odulId", out JsonElement odulIdElement))
                return BadRequest("odulId alanı eksik.");

            int odulId = odulIdElement.GetInt32(); // 🟢 int olarak al

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

            kullanici.CipBakiye -= odul.GerekliCip;
            odul.KullaniciId = kullanici.Id;
            odul.AlinmaTarihi = DateTime.UtcNow;

            _context.SaveChanges();

            return Ok("Ödül başarıyla alındı.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("🔥 Sunucu hatası: " + ex.Message);
            return StatusCode(500, "Sunucuda hata oluştu.");
        }
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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> MalzemeEkle([FromBody] GeridonusumEkleModel model)
    {
        try
        {
            // Kullanıcı oturumda mı?
            if (User == null || !User.Identity.IsAuthenticated)
                return Unauthorized("Giriş yapılmamış.");

            // Kullanıcı ID'sini claim'den al
            var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(kullaniciIdStr))
                return Unauthorized("JWT içinde kullanıcı ID'si bulunamadı.");

            if (!int.TryParse(kullaniciIdStr, out int musteriId))
                return Unauthorized("Kullanıcı ID'si geçerli değil.");

            // Yeni kayıt oluştur
            var malzeme = new GeridonusumMalzemesi
            {
                Turu = model.Turu,
                MiktarKg = model.MiktarKg,
                Tarih = DateTime.UtcNow,
                MusteriId = musteriId,
                Durum = "Beklemede" // ✅ NULL gelmesini engeller
            };

            _context.Malzemeler.Add(malzeme);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Malzeme başarıyla eklendi." });
        }
        catch (Exception ex)
        {
            Console.WriteLine("🔥 HATA:", ex);
            return StatusCode(500, "❌ Beklenmeyen hata: " + ex.Message);
        }
    }
    
    
    [HttpGet("mevcut-cip")]
    [Authorize(Roles = "musteri")]
    public IActionResult GetCipBakiye()
    {
        var email = User.FindFirstValue("name");
        var musteri = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (musteri == null)
            return NotFound("Kullanıcı bulunamadı.");

        return Ok(musteri.CipBakiye); // int ya da decimal olabilir
    }


    [Authorize]
    [HttpGet("gecmisim")]
    public async Task<IActionResult> MalzemeGecmisi()
    {
        var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(kullaniciIdStr, out int musteriId))
            return Unauthorized("Kullanıcı ID’si geçersiz.");

        var gecmis = await _context.Malzemeler
            .Where(m => m.MusteriId == musteriId)
            .OrderByDescending(m => m.Tarih)
            .Select(m => new
            {
                m.Id, 
                m.Turu,
                m.MiktarKg,
                m.Tarih,
                m.Durum,
                m.KazandigiCip
            })
            .ToListAsync();

        return Ok(gecmis);
    }


    [Authorize]
    [HttpPost("malzeme/iptal")]
    public async Task<IActionResult> MalzemeIptalEt([FromBody] int malzemeId)
    {
        var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(kullaniciIdStr, out int musteriId))
            return Unauthorized("Kullanıcı ID geçersiz.");

        var malzeme = await _context.Malzemeler
            .FirstOrDefaultAsync(m => m.Id == malzemeId && m.MusteriId == musteriId);

        if (malzeme == null)
            return NotFound(new { message = "Malzeme bulunamadı." });


        if (malzeme.Durum?.ToLower() == "iptal edildi")
            return BadRequest("Malzeme zaten iptal edilmiş.");

        malzeme.Durum = "iptal edildi";
        await _context.SaveChangesAsync();

        return Ok(new { message = "Malzeme başarıyla iptal edildi." });
    }

    
    
    

}