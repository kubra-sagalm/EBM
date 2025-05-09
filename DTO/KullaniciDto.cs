namespace EBM.DTO
{
    public class KullaniciDto
    {
        public string AdSoyad { get; set; }
        public string Email { get; set; }
        public string Sifre { get; set; }
        public string Telefon { get; set; }
        public string Adres { get; set; }
        public string Rol { get; set; }  // 'musteri', 'araci', 'firma'
    }
}