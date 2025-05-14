using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EBM.Data;
using EBM.Models;
using Microsoft.EntityFrameworkCore;

namespace EBM.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "araci")] // sadece aracı rolü erişebilir
public class AraciController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AraciController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 📦 Bekleyen geri dönüşüm malzemelerini listeler
    [HttpGet("bekleyen-malzeme-listesi")]
    public IActionResult BekleyenMalzemeleriListele()
    {
        var malzemeler = _context.Malzemeler
            .Where(m => m.Durum == "bekliyor")
            .OrderByDescending(m => m.Tarih)
            .Select(m => new
            {
                m.Id,
                m.Turu,
                m.MiktarKg,
                m.KazandigiCip,
                Tarih = m.Tarih.ToString("yyyy-MM-dd HH:mm"),
                MusteriAdSoyad = m.Musteri.AdSoyad,
                MusteriEmail = m.Musteri.Email
            })
            .ToList();

        return Ok(malzemeler);
    }
    
    [HttpPost("malzeme-bloke-et")]
    [Authorize(Roles = "araci")]
    public IActionResult MalzemeBlokeEt([FromBody] int malzemeId)
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null || araci.Rol != "araci")
            return Unauthorized("Bu işlem yalnızca aracı rolündeki kullanıcılar tarafından yapılabilir.");

        var malzeme = _context.Malzemeler.FirstOrDefault(m => m.Id == malzemeId);

        if (malzeme == null)
            return NotFound("Malzeme bulunamadı.");

        if (malzeme.Durum != "bekliyor")
            return BadRequest("Sadece bekleyen durumundaki malzemeler bloke edilebilir.");

        malzeme.Durum = "bloke edildi";
        malzeme.BlokeEdenAraciId = araci.Id; 
        _context.SaveChanges();

        return Ok("Malzeme başarıyla bloke edildi.");
    }

    
    
    [HttpGet("benim-bloke-ettiklerim")]
    [Authorize(Roles = "araci")]
    public IActionResult BenimBlokeEttiklerim()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null || araci.Rol != "araci")
            return Unauthorized("Yalnızca aracı rolündeki kullanıcılar erişebilir.");

        var blokeEdilenler = _context.Malzemeler
            .Where(m => m.Durum == "bloke edildi" && m.BlokeEdenAraciId == araci.Id)
            .OrderByDescending(m => m.Tarih)
            .Select(m => new
            {
                m.Id,
                m.Turu,
                m.MiktarKg,
                m.KazandigiCip,
                Tarih = m.Tarih.ToString("yyyy-MM-dd HH:mm"),
                MusteriAdSoyad = m.Musteri.AdSoyad,
                MusteriEmail = m.Musteri.Email
            })
            .ToList();

        return Ok(blokeEdilenler);
    }

    [HttpGet("malzeme-detay/{malzemeId}")]
    [Authorize(Roles = "araci")]
    public IActionResult BlokeEdilenMalzemeDetay(int malzemeId)
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null || araci.Rol != "araci")
            return Unauthorized("Yalnızca aracı rolündeki kullanıcılar bu sayfaya erişebilir.");

        var malzeme = _context.Malzemeler
            .Where(m => m.Id == malzemeId && m.Durum == "bloke edildi" && m.BlokeEdenAraciId == araci.Id)
            .Select(m => new
            {
                MalzemeId = m.Id,
                Turu = m.Turu,
                MiktarKg = m.MiktarKg,
                KazandigiCip = m.KazandigiCip,
                Durum = m.Durum,
                Tarih = m.Tarih.ToString("yyyy-MM-dd HH:mm"),

                Musteri = new
                {
                    m.Musteri.AdSoyad,
                    m.Musteri.Email,
                    m.Musteri.Telefon,
                    m.Musteri.Adres
                }
            })
            .FirstOrDefault();

        if (malzeme == null)
            return NotFound("Bu ID'ye sahip bloke edilmiş bir malzeme bulunamadı veya size ait değil.");

        return Ok(malzeme);
    }
    
    [HttpPost("malzeme-bloke-iptal")]
    [Authorize(Roles = "araci")]
    public IActionResult BlokeyiIptalEt([FromBody] int malzemeId)
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        var malzeme = _context.Malzemeler.FirstOrDefault(m =>
            m.Id == malzemeId &&
            m.BlokeEdenAraciId == araci.Id &&
            m.Durum == "bloke edildi");

        if (malzeme == null)
            return NotFound("Bu malzeme size ait değil veya bloke edilmemiş.");

        malzeme.Durum = "bekliyor";
        malzeme.BlokeEdenAraciId = null;

        _context.SaveChanges();

        return Ok("Malzemenin blokesi iptal edildi.");
    }


    [HttpPost("malzeme-satin-al")]
    [Authorize(Roles = "araci")]
    public IActionResult MalzemeSatinAl([FromBody] int malzemeId)
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Kullanıcı doğrulanamadı.");

        var malzeme = _context.Malzemeler
            .Include(m => m.Musteri)
            .FirstOrDefault(m =>
                m.Id == malzemeId &&
                m.BlokeEdenAraciId == araci.Id &&
                m.Durum == "bloke edildi");

        if (malzeme == null)
            return NotFound("Malzeme bulunamadı ya da size ait değil.");

        // Çip ve para hesaplama
        int verilenCip = (int)(malzeme.MiktarKg * 10);
        decimal paraKarsiligi = verilenCip / 10.0m;

        // Müşteriye kazanç ekle
        var musteri = malzeme.Musteri;
        musteri.CipBakiye += verilenCip;
        musteri.ParaBakiye = (musteri.ParaBakiye ?? 0) + paraKarsiligi;

        // Malzeme durumu güncelle
        malzeme.Durum = "satildi";

        // SatinAlim tablosuna kayıt
        var satinAlim = new SatinAlim
        {
            AraciId = araci.Id,
            MalzemeId = malzeme.Id,
            Tarih = DateTime.UtcNow,
            VerilenCip = verilenCip
        };

        _context.SatinAlimlar.Add(satinAlim);
        _context.SaveChanges();

        return Ok(new
        {
            mesaj = "Malzeme başarıyla satın alındı ve kayıt edildi.",
            musteri = musteri.AdSoyad,
            kazandigiCip = verilenCip,
            paraKarsiligi = paraKarsiligi
        });
    }

    
    [HttpGet("satin-aldiklarim-ozet")]
    [Authorize(Roles = "araci")]
    public IActionResult SatinAldiklarimOzet()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null || araci.Rol != "araci")
            return Unauthorized("Aracı kullanıcı bulunamadı.");

        var ozet = _context.SatinAlimlar
            .Where(sa => sa.AraciId == araci.Id)
            .Include(sa => sa.Malzeme)
            .GroupBy(sa => sa.Malzeme.Turu)
            .Select(g => new
            {
                Turu = g.Key,
                ToplamKg = g.Sum(sa => sa.Malzeme.MiktarKg)
            })
            .ToList();

        return Ok(ozet);
    }
    
    [HttpPost("acik-artirmaya-gonder")]
    [Authorize(Roles = "araci")]
    public IActionResult AcikArtirmayaGonder([FromBody] AcikArtirmaGonderModel model)
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Aracı kullanıcı bulunamadı.");

        var satinAlim = _context.SatinAlimlar
            .Include(sa => sa.Malzeme)
            .FirstOrDefault(sa => sa.Id == model.SatinAlimId && sa.AraciId == araci.Id);

        if (satinAlim == null)
            return BadRequest("Bu satın alma size ait değil veya bulunamadı.");

        var malzemeId = satinAlim.MalzemeId;

        bool zatenVar = _context.AcikArtirmalar.Any(a => a.MalzemeId == malzemeId);
        if (zatenVar)
            return BadRequest("Bu malzeme zaten açık artırmada.");

        var now = DateTime.UtcNow;

        var yeniArtirma = new AcikArtirma
        {
            AraciId = araci.Id,
            MalzemeId = malzemeId,
            BaslangicTarihi = now,
            BitisTarihi = now.AddDays(1),
            Durum = "aktif"
        };

        _context.AcikArtirmalar.Add(yeniArtirma);
        _context.SaveChanges();

        return Ok("Malzeme açık artırmaya gönderildi.");
    }
    
    [HttpGet("acik-artirmalarim")]
    [Authorize(Roles = "araci")]
    public IActionResult AcikArtirmalarim()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Aracı kullanıcı bulunamadı.");

        var liste = _context.AcikArtirmalar
            .Where(a => a.AraciId == araci.Id)
            .Include(a => a.Malzeme)
            .OrderByDescending(a => a.BaslangicTarihi)
            .Select(a => new
            {
                AcikArtirmaId = a.Id,
                MalzemeId = a.Malzeme.Id,
                Turu = a.Malzeme.Turu,
                MiktarKg = a.Malzeme.MiktarKg,
                KazandigiCip = a.Malzeme.KazandigiCip,
                MalzemeDurumu = a.Malzeme.Durum,
                AcikArtirmaDurumu = a.Durum,
                BaslangicTarihi = a.BaslangicTarihi.ToString("yyyy-MM-dd HH:mm"),
                BitisTarihi = a.BitisTarihi.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        return Ok(liste);
    }
    
    [HttpGet("cip-bakiye-ve-gecmis")]
    [Authorize(Roles = "araci")]
    public IActionResult CipBakiyesiVeGecmis()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Kullanıcı bulunamadı.");

        var satinAlimlar = _context.SatinAlimlar
            .Include(sa => sa.Malzeme)
            .Where(sa => sa.AraciId == araci.Id)
            .OrderByDescending(sa => sa.Tarih)
            .Select(sa => new
            {
                MalzemeTuru = sa.Malzeme.Turu,
                MiktarKg = sa.Malzeme.MiktarKg,
                VerilenCip = sa.VerilenCip,
                Tarih = sa.Tarih.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        return Ok(new
        {
            ToplamCipBakiyesi = araci.CipBakiye,
            SatinAlimGecmisi = satinAlimlar
        });
    }

    [HttpGet("acik-artirma-tekliflerim")]
    [Authorize(Roles = "araci")]
    public IActionResult AcikArtirmaTekliflerim()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Kullanıcı bulunamadı.");

        var teklifler = _context.AcikArtirmalar
            .Where(a => a.AraciId == araci.Id)
            .Include(a => a.Malzeme)
            .Include(a => a.Teklifler)
            .ThenInclude(t => t.Firma)
            .Select(a => new
            {
                AcikArtirmaId = a.Id,
                MalzemeTuru = a.Malzeme.Turu,
                MiktarKg = a.Malzeme.MiktarKg,
                KazandigiCip = a.Malzeme.KazandigiCip,
                BaslangicTarihi = a.BaslangicTarihi.ToString("yyyy-MM-dd HH:mm"),
                BitisTarihi = a.BitisTarihi.ToString("yyyy-MM-dd HH:mm"),
                AcikArtirmaDurumu = a.Durum,

                EnYuksekTeklif = a.Teklifler
                    .OrderByDescending(t => t.TeklifTutar)
                    .Select(t => new
                    {
                        TeklifTutar = t.TeklifTutar,
                        Tarih = t.Tarih.ToString("yyyy-MM-dd HH:mm"),
                        FirmaAd = t.Firma.AdSoyad,
                        FirmaEmail = t.Firma.Email
                    })
                    .FirstOrDefault()
            })
            .ToList();

        return Ok(teklifler);
    }
    
    [HttpGet("bekleyen-teklifler")]
    [Authorize(Roles = "araci")]
    public IActionResult BekleyenTeklifler()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Aracı kullanıcı bulunamadı.");

        // Sadece bu aracının açık artırmalarına gelen, durumu "bekliyor" olan teklifler
        var teklifler = _context.Teklifler
            .Include(t => t.AcikArtirma)
            .ThenInclude(a => a.Malzeme)
            .Include(t => t.Firma)
            .Where(t =>
                t.Durum == "bekliyor" &&
                t.AcikArtirma.AraciId == araci.Id)
            .OrderByDescending(t => t.Tarih)
            .Select(t => new
            {
                TeklifId = t.Id,
                TeklifTutar = t.TeklifTutar,
                Tarih = t.Tarih.ToString("yyyy-MM-dd HH:mm"),
                FirmaAd = t.Firma.AdSoyad,
                FirmaEmail = t.Firma.Email,

                Malzeme = new
                {
                    t.AcikArtirma.Malzeme.Turu,
                    t.AcikArtirma.Malzeme.MiktarKg,
                    t.AcikArtirma.Malzeme.KazandigiCip
                },

                AcikArtirmaId = t.AcikArtirmaId,
                AcikArtirmaDurumu = t.AcikArtirma.Durum
            })
            .ToList();

        return Ok(teklifler);
    }

    
    [HttpGet("teklif-detay/{teklifId}")]
    [Authorize(Roles = "araci")]
    public IActionResult TeklifDetay(int teklifId)
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Kullanıcı doğrulanamadı.");

        var teklif = _context.Teklifler
            .Include(t => t.Firma)
            .Include(t => t.AcikArtirma)
            .ThenInclude(a => a.Malzeme)
            .FirstOrDefault(t =>
                t.Id == teklifId &&
                t.AcikArtirma.AraciId == araci.Id); // sadece kendi malzemesi kontrolü

        if (teklif == null)
            return NotFound("Bu teklif size ait bir açık artırma ile ilişkili değil.");

        var detay = new
        {
            TeklifId = teklif.Id,
            TeklifTutar = teklif.TeklifTutar,
            Tarih = teklif.Tarih.ToString("yyyy-MM-dd HH:mm"),
            Durum = teklif.Durum,

            Firma = new
            {
                AdSoyad = teklif.Firma.AdSoyad,
                Email = teklif.Firma.Email,
                Telefon = teklif.Firma.Telefon,
                Adres = teklif.Firma.Adres
            },

            Malzeme = new
            {
                Turu = teklif.AcikArtirma.Malzeme.Turu,
                MiktarKg = teklif.AcikArtirma.Malzeme.MiktarKg,
                Durum = teklif.AcikArtirma.Malzeme.Durum,
                KazandigiCip = teklif.AcikArtirma.Malzeme.KazandigiCip
            }
        };

        return Ok(detay);
    }




    
    
}