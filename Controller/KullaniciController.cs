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

        // POST: api/kullanici
        [HttpPost]
        public IActionResult CreateUser([FromBody] KullaniciCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var kullanici = new Kullanici
            {
                AdSoyad = dto.AdSoyad,
                Email = dto.Email,
                Sifre = dto.Sifre,
                Telefon = dto.Telefon,
                Adres = dto.Adres,
                Rol = dto.Rol,
                CipBakiye = dto.CipBakiye ?? 0,
                ParaBakiye = dto.ParaBakiye ?? 0
            };

            _context.Kullanicilar.Add(kullanici);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetUserById), new { id = kullanici.Id }, kullanici);
        }

        // GET: api/kullanici/5
        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            var user = _context.Kullanicilar.Find(id);
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}