using Microsoft.EntityFrameworkCore;
using EBM.Models;

namespace EBM.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<GeridonusumMalzemesi> Malzemeler { get; set; }
        public DbSet<SatinAlim> SatinAlimlar { get; set; }
        public DbSet<AcikArtirma> AcikArtirmalar { get; set; }
        public DbSet<Teklif> Teklifler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Kullanici -> GeridonusumMalzemesi (1-m)
            modelBuilder.Entity<GeridonusumMalzemesi>()
                .HasOne(m => m.Musteri)
                .WithMany(k => k.Malzemeler)
                .HasForeignKey(m => m.MusteriId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kullanici -> SatinAlim (1-m) (Araci)
            modelBuilder.Entity<SatinAlim>()
                .HasOne(sa => sa.Araci)
                .WithMany(k => k.SatinAlimlar)
                .HasForeignKey(sa => sa.AraciId)
                .OnDelete(DeleteBehavior.Restrict);

            // SatinAlim -> Malzeme (1-1)
            modelBuilder.Entity<SatinAlim>()
                .HasOne(sa => sa.Malzeme)
                .WithOne(m => m.SatinAlim)
                .HasForeignKey<SatinAlim>(sa => sa.MalzemeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kullanici -> AcikArtirma (1-m) (Araci)
            modelBuilder.Entity<AcikArtirma>()
                .HasOne(a => a.Araci)
                .WithMany(k => k.AcikArtirmalar)
                .HasForeignKey(a => a.AraciId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kullanici -> AcikArtirma (1-1) (KazananFirma) [ters yön yok]
            modelBuilder.Entity<AcikArtirma>()
                .HasOne(a => a.KazananFirma)
                .WithMany() // Kullanici'de Kazanilanlar listesi yok
                .HasForeignKey(a => a.KazananFirmaId)
                .OnDelete(DeleteBehavior.Restrict);

            // AcikArtirma -> Malzeme (1-1)
            modelBuilder.Entity<AcikArtirma>()
                .HasOne(a => a.Malzeme)
                .WithOne(m => m.AcikArtirma)
                .HasForeignKey<AcikArtirma>(a => a.MalzemeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Teklif -> Firma (1-m)
            modelBuilder.Entity<Teklif>()
                .HasOne(t => t.Firma)
                .WithMany(k => k.Teklifler)
                .HasForeignKey(t => t.FirmaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Teklif -> AcikArtirma (1-m)
            modelBuilder.Entity<Teklif>()
                .HasOne(t => t.AcikArtirma)
                .WithMany(a => a.Teklifler)
                .HasForeignKey(t => t.AcikArtirmaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
