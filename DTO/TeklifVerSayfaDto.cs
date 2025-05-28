namespace EBM.DTO;

public class TeklifVerSayfaDto
{
    public int AcikArtirmaId { get; set; }
    public string MalzemeAdi { get; set; }
    public float MiktarKg { get; set; } // ya da double, hangisiyse modelde

    public string AraciAdi { get; set; }
    public string AraciTelefon { get; set; } // ✅ yeni
    public decimal? EnYuksekTeklif { get; set; }
    public int ToplamTeklifSayisi { get; set; }
    public string FirmaAdi { get; set; }
    public decimal? KendiTeklifTutar { get; set; }
    public string KendiTeklifDurumu { get; set; }
}
