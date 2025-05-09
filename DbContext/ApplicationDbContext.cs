using EBM.Models;
using Microsoft.EntityFrameworkCore;


namespace EBM.DbContext
{
    public class ApplicationDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<GeridonusumMalzemesi> GeridonusumMalzemeleri { get; set; }
        public DbSet<SatinAlim> SatinAlimlar { get; set; }
        public DbSet<AcikArtirma> AcikArtirmalar { get; set; }
        public DbSet<Teklif> Teklifler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeridonusumMalzemesi>()
                .HasOne(m => m.Musteri)
                .WithMany(k => k.Malzemeler)
                .HasForeignKey(m => m.MusteriId);

            modelBuilder.Entity<SatinAlim>()
                .HasOne(s => s.Araci)
                .WithMany(k => k.SatinAlimlar)
                .HasForeignKey(s => s.AraciId);

            modelBuilder.Entity<SatinAlim>()
                .HasOne(s => s.Malzeme)
                .WithMany()
                .HasForeignKey(s => s.MalzemeId);

            modelBuilder.Entity<AcikArtirma>()
                .HasOne(a => a.Araci)
                .WithMany(k => k.AcikArtirmalar)
                .HasForeignKey(a => a.AraciId);

            modelBuilder.Entity<AcikArtirma>()
                .HasOne(a => a.Malzeme)
                .WithMany()
                .HasForeignKey(a => a.MalzemeId);

            modelBuilder.Entity<AcikArtirma>()
                .HasOne(a => a.KazananFirma)
                .WithMany()
                .HasForeignKey(a => a.KazananFirmaId)
                .IsRequired(false);

            modelBuilder.Entity<Teklif>()
                .HasOne(t => t.Firma)
                .WithMany()
                .HasForeignKey(t => t.FirmaId);

            modelBuilder.Entity<Teklif>()
                .HasOne(t => t.AcikArtirma)
                .WithMany(a => a.Teklifler)
                .HasForeignKey(t => t.AcikArtirmaId);
        }
    }
}
