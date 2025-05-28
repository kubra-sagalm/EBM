using EBM.Data;
using EBM.DTO;
using EBM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EBM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirmaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FirmaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("acikartirmalar")]
        public async Task<IActionResult> GetAcikArtirmaListesi()
        {
            var acikArtirmalar = await _context.AcikArtirmalar
                .Include(a => a.Malzeme)
                .Include(a => a.Teklifler)
                .Where(a => a.Durum == "aktif" && a.BitisTarihi > DateTime.UtcNow)
                .Select(a => new AcikArtirmaListeDto
                {
                    AcikArtirmaId = a.Id,
                    MalzemeTuru = a.Malzeme.Turu,
                    MiktarKg = a.Malzeme.MiktarKg,
                    SonTeklif = a.Teklifler.OrderByDescending(t => t.TeklifTutar).Select(t => (decimal?)t.TeklifTutar).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(acikArtirmalar);
        }

// FirmaController.cs

        [HttpGet("teklif-gecmisi")]
        [Authorize(Roles = "firma")]
        public async Task<IActionResult> TeklifGecmisi()
        {
            var email = User.FindFirst(ClaimTypes.Name)?.Value;
            if (email == null)
                return Unauthorized("Kullanıcı kimliği doğrulanamadı.");

            var firma = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Email == email);
            if (firma == null)
                return NotFound("Firma bulunamadı.");

            var teklifler = await _context.Teklifler
                .Where(t => t.FirmaId == firma.Id)
                .Include(t => t.AcikArtirma)
                .ThenInclude(a => a.Malzeme)
                .Select(t => new
                {
                    malzemeTuru = t.AcikArtirma.Malzeme.Turu,
                    malzemeMiktari = t.AcikArtirma.Malzeme.MiktarKg,
                    teklifTutar = t.TeklifTutar,
                    tarih = t.Tarih,
                    durum = t.Durum
                })
                .ToListAsync();

            return Ok(teklifler); // ❗ Hangi durumda olursa olsun JSON liste döner
        }

        
        
        [HttpGet("verisayfa/{acikArtirmaId}")]
        [Authorize(Roles = "firma")]
        public async Task<IActionResult> TeklifVerSayfa(int acikArtirmaId)
        {
            var email = User.FindFirst("name")?.Value;

            var firma = await _context.Kullanicilar
                .FirstOrDefaultAsync(x => x.Email == email && x.Rol == "firma");

            if (firma == null)
                return Unauthorized("Firma bilgisi bulunamadı.");

            var acikArtirma = await _context.AcikArtirmalar
                .Include(x => x.Malzeme)
                .Include(x => x.Araci)
                .Include(x => x.Teklifler)
                .FirstOrDefaultAsync(x => x.Id == acikArtirmaId && x.Durum == "aktif");

            if (acikArtirma == null)
                return NotFound("Açık artırma bulunamadı veya aktif değil.");

            var kendiTeklif = acikArtirma.Teklifler
                .Where(t => t.FirmaId == firma.Id)
                .OrderByDescending(t => t.Tarih)
                .FirstOrDefault();

            var dto = new TeklifVerSayfaDto
            {
                AcikArtirmaId = acikArtirma.Id,
                MalzemeAdi = acikArtirma.Malzeme.Turu,
                MiktarKg = acikArtirma.Malzeme.MiktarKg, // ✅ eklendi
                AraciAdi = acikArtirma.Araci.AdSoyad,
                AraciTelefon = acikArtirma.Araci.Telefon,
                EnYuksekTeklif = acikArtirma.Teklifler.Max(t => (decimal?)t.TeklifTutar),
                ToplamTeklifSayisi = acikArtirma.Teklifler.Count,
                FirmaAdi = firma.AdSoyad,
                KendiTeklifTutar = kendiTeklif?.TeklifTutar,
                KendiTeklifDurumu = kendiTeklif?.Durum
            };
            Console.WriteLine("Araci Telefon: " + acikArtirma.Araci.Telefon);


            return Ok(dto);
        }

        
        [HttpPost("ver")]
        [Authorize(Roles = "firma")]
        public async Task<IActionResult> TeklifVer([FromBody] TeklifVerDto dto)
        {
            var email = User.FindFirst("name")?.Value;

            var firma = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.Email == email && k.Rol == "firma");

            if (firma == null)
                return Unauthorized("Firma bulunamadı.");

            var acikArtirma = await _context.AcikArtirmalar
                .Include(a => a.Teklifler)
                .FirstOrDefaultAsync(a => a.Id == dto.AcikArtirmaId && a.Durum == "aktif");

            if (acikArtirma == null)
                return NotFound("Açık artırma bulunamadı.");

            var mevcutEnYuksek = acikArtirma.Teklifler.Max(t => (decimal?)t.TeklifTutar) ?? 0;

            if (dto.TeklifTutar <= mevcutEnYuksek)
                return BadRequest($"Yeni teklif, mevcut en yüksek tekliften yüksek olmalıdır. (Şu anki en yüksek: {mevcutEnYuksek})");

            // Önceki tekliflerin durumunu "kaybetti" olarak güncelle
            foreach (var teklif in acikArtirma.Teklifler)
            {
                teklif.Durum = "kaybetti";
            }

            // Yeni teklifi oluştur
            var yeniTeklif = new Teklif
            {
                AcikArtirmaId = acikArtirma.Id,
                FirmaId = firma.Id,
                TeklifTutar = dto.TeklifTutar,
                Tarih = DateTime.UtcNow,
                Durum = "geçerli"
            };

            _context.Teklifler.Add(yeniTeklif);
            await _context.SaveChangesAsync();

            return Ok("Teklif başarıyla verildi.");
        }


    }
    
}