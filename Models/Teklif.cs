namespace EBM.Models;
public class Teklif
{
    public int Id { get; set; }

    public int FirmaId { get; set; }  // Bu alan artık "teklifi veren kullanıcı"yı temsil eder
    public Kullanici Firma { get; set; }  // navigation property

    public int AcikArtirmaId { get; set; }
    public AcikArtirma AcikArtirma { get; set; }

    public decimal TeklifTutar { get; set; }
    public DateTime Tarih { get; set; }

    public string Durum { get; set; }
}
