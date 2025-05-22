using System.ComponentModel.DataAnnotations.Schema;

namespace EBM.Models;

public class GeridonusumMalzemesi
{
    public int Id { get; set; }
    public int MusteriId { get; set; }
    public Kullanici Musteri { get; set; }

    public int? BlokeEdenAraciId { get; set; }


    public Kullanici BlokeEdenAraci { get; set; }
    public DateTime? BlokeEdilmeTarihi { get; set; } // nullable olsun çünkü ilk başta boş



    public string Turu { get; set; }
    public float MiktarKg { get; set; }
    public DateTime Tarih { get; set; }
    public string Durum { get; set; } // 'bekliyor', 'satildi', 'iptal'
    public int KazandigiCip { get; set; }

    public SatinAlim SatinAlim { get; set; }
    public AcikArtirma AcikArtirma { get; set; }
}