namespace EBM.Models;

public class SatinAlim
{
    public int Id { get; set; }
    public int AraciId { get; set; }
    public Kullanici Araci { get; set; }

    public int MalzemeId { get; set; }
    public GeridonusumMalzemesi Malzeme { get; set; }

    public DateTime Tarih { get; set; }
    public int VerilenCip { get; set; }

    public ICollection<AcikArtirma> AcikArtirmalar { get; set; }
}