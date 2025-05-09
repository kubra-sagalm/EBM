using Microsoft.AspNetCore.Mvc;
using EBM.DbContext;
using EBM.Models;
using EBM.DTO;

namespace EBM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KullaniciController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KullaniciController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpPost("kayit")]
        public IActionResult Kayit([FromBody] KullaniciDto dto)
        {
            // Aynı e-posta + aynı rol ile kayıt varsa engelle
            var ayniRoldeKayitVarMi = _context.Kullanicilar
                .Any(k => k.Email == dto.Email && k.Rol == dto.Rol);

            if (ayniRoldeKayitVarMi)
            {
                return BadRequest("Bu e-posta adresiyle bu rol için zaten kayıt yapılmış.");
            }

            var kullanici = new Kullanici
            {
                AdSoyad = dto.AdSoyad,
                Email = dto.Email,
                Sifre = BCrypt.Net.BCrypt.HashPassword(dto.Sifre),
                Telefon = dto.Telefon,
                Adres = dto.Adres,
                Rol = dto.Rol,
                CipBakiye = dto.Rol == "Müşteri" || dto.Rol == "Aracı" ? 0 : null,
                ParaBakiye = dto.Rol == "Firma" ? 0 : null
            };

            _context.Kullanicilar.Add(kullanici);
            _context.SaveChanges();

            return Ok(new { mesaj = "Kayıt başarılı", kullanici.Id });
        }
    }
    

}