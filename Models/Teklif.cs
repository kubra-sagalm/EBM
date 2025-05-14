namespace EBM.Models;

public class Teklif
{
    public int Id { get; set; }

    public int FirmaId { get; set; }
    public Kullanici Firma { get; set; }

    public int AcikArtirmaId { get; set; }
    public AcikArtirma AcikArtirma { get; set; }

    public decimal TeklifTutar { get; set; }
    public DateTime Tarih { get; set; }
    public string Durum { get; set; } // 'kazandi', 'kaybetti', 'bekliyor'
}