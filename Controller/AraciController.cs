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

    [Authorize(Roles = "araci")]
    [HttpGet("bekleyen-malzeme-listesi")]
    public async Task<IActionResult> BekleyenMalzemeler()
    {
        var malzemeler = await _context.Malzemeler
            .Where(m => m.Durum == "Beklemede")
            .Include(m => m.Musteri)
            .Select(m => new
            {
                m.Id,
                m.Turu,
                m.MiktarKg,
                m.Tarih,
                musteriId = m.MusteriId,
                musteriAdSoyad = m.Musteri.AdSoyad,
                musteriAdres = m.Musteri.Adres,
                musteriTelefon = m.Musteri.Telefon
            })
            .ToListAsync();

        return Ok(malzemeler);
    }

    
    [HttpPost("malzeme-bloke-et")]
    [Authorize(Roles = "araci")]
    public IActionResult MalzemeBlokeEt([FromBody] int malzemeId)
    {
        // JWT içinden e-posta bilgisi alınır
        var email = User.FindFirstValue("name");
        if (string.IsNullOrEmpty(email))
            return Unauthorized("Kullanıcı bilgisi alınamadı.");

        // Giriş yapan kullanıcı aracı mı kontrolü
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email && k.Rol == "araci");
        if (araci == null)
            return Unauthorized("Bu işlem yalnızca aracı kullanıcılar tarafından yapılabilir.");

        // Malzeme kontrolü
        var malzeme = _context.Malzemeler.FirstOrDefault(m => m.Id == malzemeId);
        if (malzeme == null)
            return NotFound("Malzeme bulunamadı.");

        if (malzeme.Durum != "Beklemede")
            return BadRequest("Sadece 'Beklemede' durumundaki malzemeler bloke edilebilir.");

        // Bloke işlemi
        malzeme.Durum = "bloke edildi";
        malzeme.BlokeEdenAraciId = araci.Id; // ✅ giriş yapan kullanıcının ID'si atanıyor
        malzeme.Tarih = DateTime.UtcNow;     // ❗ opsiyonel: bloke zamanı güncellenebilir
        malzeme.BlokeEdilmeTarihi = DateTime.UtcNow; // UTC kullanımı önerilir

        _context.SaveChanges();

        return Ok("Malzeme başarıyla bloke edildi.");
    }


    [HttpGet("benim-bloke-ettiklerim")]
    [Authorize(Roles = "araci")]
    public IActionResult BenimBlokeEttiklerim()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized();

        var now = DateTime.UtcNow;

        var malzemeler = _context.Malzemeler
            .Include(m => m.Musteri)
            .Where(m => m.BlokeEdenAraciId == araci.Id && m.Durum == "bloke edildi")
            .ToList();

        foreach (var m in malzemeler)
        {
            if (m.BlokeEdilmeTarihi.HasValue && now > m.BlokeEdilmeTarihi.Value.AddHours(2))
            {
                m.Durum = "Bekliyor";
                m.BlokeEdenAraciId = null;
                m.BlokeEdilmeTarihi = null;
            }
        }

        _context.SaveChanges();

        var result = malzemeler
            .Where(m => m.Durum == "bloke edildi") // Güncellenenlerin dışındaki kalanlar
            .Select(m => new
            {
                m.Id,
                m.Turu,
                m.MiktarKg,
                m.KazandigiCip,
                m.Durum,
                m.BlokeEdilmeTarihi,
                MusteriAdSoyad = m.Musteri.AdSoyad
            }).ToList();

        return Ok(result);
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
    
    [HttpGet("mevcut-cip")]
    [Authorize(Roles = "araci")]
    public IActionResult GetCipBakiyesi()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return NotFound();

        return Ok(araci.CipBakiye); // örn: 475
    }

    [HttpPost("malzeme-bloke-iptal")]
    [Authorize(Roles = "araci")]
    public IActionResult BlokeyiIptalEt([FromBody] int malzemeId)
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        var malzeme = _context.Malzemeler.FirstOrDefault(m =>
            m.Id == malzemeId &&
            m.BlokeEdenAraciId == araci.Id);

        if (malzeme == null)
            return NotFound("Bu malzeme size ait değil veya bulunamadı.");

        // 🔒 Satıldıysa işlem yapılmasın
        if (malzeme.Durum == "satildi")
            return BadRequest("Bu malzeme satılmış. Bloke iptali yapılamaz.");

        // 🔒 Sadece 'bloke edildi' durumundakiler iptal edilebilir
        if (malzeme.Durum != "bloke edildi")
            return BadRequest("Bu malzeme şu anda bloke durumda değil.");

        // Bloke iptal işlemi
        malzeme.Durum = "Beklemede";
        malzeme.BlokeEdenAraciId = null;
        malzeme.BlokeEdilmeTarihi = null;

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

        // Gerekli çip hesaplama (örneğin 1 kg = 10 çip)
        int gerekenCip = (int)(malzeme.MiktarKg * 10);
        decimal paraKarsiligi = gerekenCip / 10.0m;

        if (araci.CipBakiye < gerekenCip)
        {
            return BadRequest($"Yetersiz çip bakiyesi. Satın alma için {gerekenCip} çip gerekir, sizin bakiyeniz {araci.CipBakiye}.");
        }

        // Müşteriye çip ve para ekle
        var musteri = malzeme.Musteri;
        musteri.CipBakiye += gerekenCip;
        musteri.ParaBakiye = (musteri.ParaBakiye ?? 0) + paraKarsiligi;

        // Aracıdan çip düş
        araci.CipBakiye -= gerekenCip;

        // Malzeme durumu güncelle
        malzeme.Durum = "satildi";

        // SatinAlim kaydı
        var satinAlim = new SatinAlim
        {
            AraciId = araci.Id,
            MalzemeId = malzeme.Id,
            Tarih = DateTime.UtcNow,
            VerilenCip = gerekenCip
        };

        _context.SatinAlimlar.Add(satinAlim);
        _context.SaveChanges();

        return Ok(new
        {
            mesaj = "✅ Malzeme başarıyla satın alındı.",
            musteri = musteri.AdSoyad,
            kazandigiCip = gerekenCip,
            paraKarsiligi = paraKarsiligi,
            kalanCip = araci.CipBakiye
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

        var satinAlinanlar = _context.SatinAlimlar
            .Include(sa => sa.Malzeme)
            .Where(sa => sa.AraciId == araci.Id)
            .ToList();

        // Sadece açık artırmada olmayanları al
        var filtrelenmis = satinAlinanlar
            .Where(sa => !_context.AcikArtirmalar.Any(a => a.MalzemeId == sa.MalzemeId))
            .Select(sa => new
            {
                SatinAlimId = sa.Id,
                Turu = sa.Malzeme.Turu,
                ToplamKg = sa.Malzeme.MiktarKg
            })
            .ToList();

        return Ok(filtrelenmis);
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
    
    [HttpGet("satin-alim-gecmisim")]
    [Authorize(Roles = "araci")]
    public IActionResult SatinAlimGecmisim()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Kullanıcı bulunamadı.");

        var gecmis = _context.SatinAlimlar
            .Where(sa => sa.AraciId == araci.Id)
            .Include(sa => sa.Malzeme)
            .OrderByDescending(sa => sa.Tarih)
            .Select(sa => new
            {
                MalzemeTuru = sa.Malzeme.Turu,
                MiktarKg = sa.Malzeme.MiktarKg,
                VerilenCip = sa.VerilenCip,
                Tarih = sa.Tarih.ToString("yyyy-MM-dd HH:mm"),
                MalzemeDurumu = sa.Malzeme.Durum
            })
            .ToList();

        return Ok(gecmis);
    }

    
    [HttpGet("odul-harcama-gecmisi")]
    [Authorize(Roles = "araci")]
    public IActionResult OdulHarcamaGecmisi()
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Aracı kullanıcı bulunamadı.");

        var harcamaListesi = _context.Oduller
            .Where(o => o.KullaniciId == araci.Id && o.AlinmaTarihi != null)
            .OrderByDescending(o => o.AlinmaTarihi)
            .Select(o => new
            {
                OdulAdi = o.Ad,
                HarcananCip = o.GerekliCip,
                AlinmaTarihi = o.AlinmaTarihi.Value.ToString("yyyy-MM-dd HH:mm")
            })
            .ToList();

        return Ok(harcamaListesi);
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
    
    
    [HttpGet("acik-artirma-detay/{acikArtirmaId}")]
    [Authorize(Roles = "araci")]
    public IActionResult AcikArtirmaDetay(int acikArtirmaId)
    {
        var email = User.FindFirstValue("name");
        var araci = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (araci == null)
            return Unauthorized("Kullanıcı doğrulanamadı.");

        var teklif = _context.Teklifler
            .Include(t => t.Firma)
            .Include(t => t.AcikArtirma)
            .ThenInclude(a => a.Malzeme)
            .Where(t =>
                t.AcikArtirmaId == acikArtirmaId &&
                t.AcikArtirma.AraciId == araci.Id)
            .OrderByDescending(t => t.TeklifTutar) // en yüksek teklifi al
            .FirstOrDefault();

        if (teklif == null)
            return NotFound("Bu açık artırma size ait değil veya teklif yok.");

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