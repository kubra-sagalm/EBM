namespace EBM.Models;

public class Odul
{
    public int Id { get; set; }
    public string Ad { get; set; }
    public int GerekliCip { get; set; }
    public int? KullaniciId { get; set; } // nullable çünkü henüz kimse almadıysa null olur
    public Kullanici Kullanici { get; set; }

    public DateTime? AlinmaTarihi { get; set; } // Ödül alındıysa zamanını da tut
}