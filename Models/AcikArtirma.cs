namespace EBM.Models;

public class AcikArtirma
{
    public int Id { get; set; }
    public int AraciId { get; set; }
    public Kullanici Araci { get; set; }

    public int MalzemeId { get; set; }
    public GeridonusumMalzemesi Malzeme { get; set; }

    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string Durum { get; set; } // 'aktif', 'tamamlandi', 'iptal'

    public int? KazananFirmaId { get; set; }
    public Kullanici KazananFirma { get; set; }

    public decimal? KazanilanFiyat { get; set; }
    public ICollection<Teklif> Teklifler { get; set; }
}