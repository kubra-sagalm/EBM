namespace EBM.Models;

public class GeridonusumMalzemesi
{
    public int Id { get; set; }
    public int MusteriId { get; set; }
    public Kullanici Musteri { get; set; }
    public int? BlokeEdenAraciId { get; set; } // Nullable yap, istersen zorunluysa int


    public string Turu { get; set; }
    public float MiktarKg { get; set; }
    public DateTime Tarih { get; set; }
    public string Durum { get; set; } // 'bekliyor', 'satildi', 'iptal'
    public int KazandigiCip { get; set; }

    public SatinAlim SatinAlim { get; set; }
    public AcikArtirma AcikArtirma { get; set; }
}