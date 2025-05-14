using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using EBM.Data;
using EBM.Models;

namespace EBM.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeridonusumController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GeridonusumController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("ekle")]
    [Authorize]
    public IActionResult MalzemeEkle([FromBody] GeridonusumEkleModel model)
    {
        // Token içindeki kullanıcı e-posta bilgisini al
        var email = User.FindFirstValue("name");
        var user = _context.Kullanicilar.FirstOrDefault(k => k.Email == email);

        if (user == null)
            return Unauthorized("Kullanıcı bulunamadı.");

        if (user.Rol != "musteri")
            return Forbid("Bu işlem sadece müşteri rolündeki kullanıcılar için geçerlidir.");

        // Cip kazanımı hesapla (örnek: 1 kg = 10 cip)
        int cip = (int)(model.MiktarKg * 10);

        var yeniMalzeme = new GeridonusumMalzemesi
        {
            MusteriId = user.Id,
            Turu = model.Turu,
            MiktarKg = model.MiktarKg,
            Tarih = DateTime.UtcNow,
            Durum = "bekliyor",
            KazandigiCip = cip
        };

        _context.Malzemeler.Add(yeniMalzeme);
        user.CipBakiye += cip;
        _context.SaveChanges();

        return Ok("Malzeme başarıyla eklendi ve cip bakiyeniz güncellendi.");
    }
}