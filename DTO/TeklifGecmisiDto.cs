namespace EBM.DTO;

public class TeklifGecmisiDto
{
    public string MalzemeTuru { get; set; }
    public float MiktarKg { get; set; }
    public DateTime Tarih { get; set; }
    public string AliciAd { get; set; }
    public string Adres { get; set; }
    public string Telefon { get; set; }
    public decimal TeklifTutar { get; set; }
    public string Durum { get; set; }
}