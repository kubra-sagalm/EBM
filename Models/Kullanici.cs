namespace EBM.Models;

public class Kullanici
{
    public int Id { get; set; }
    public string AdSoyad { get; set; }
    public string Email { get; set; }
    public string Sifre { get; set; }
    public string Telefon { get; set; }
    public string Adres { get; set; }
    public string Rol { get; set; } // 'musteri', 'araci', 'firma'
    public int? CipBakiye { get; set; }
    public decimal? ParaBakiye { get; set; }

    public ICollection<GeridonusumMalzemesi> Malzemeler { get; set; }
    public ICollection<SatinAlim> SatinAlimlar { get; set; }
    public ICollection<AcikArtirma> AcikArtirmalar { get; set; }
    public ICollection<Teklif> Teklifler { get; set; }
}
