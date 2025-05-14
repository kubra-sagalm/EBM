using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EBM.Migrations
{
    /// <inheritdoc />
    public partial class IlkMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdSoyad = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Sifre = table.Column<string>(type: "text", nullable: false),
                    Telefon = table.Column<string>(type: "text", nullable: false),
                    Adres = table.Column<string>(type: "text", nullable: false),
                    Rol = table.Column<string>(type: "text", nullable: false),
                    CipBakiye = table.Column<int>(type: "integer", nullable: true),
                    ParaBakiye = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Malzemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MusteriId = table.Column<int>(type: "integer", nullable: false),
                    Turu = table.Column<string>(type: "text", nullable: false),
                    MiktarKg = table.Column<float>(type: "real", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<string>(type: "text", nullable: false),
                    KazandigiCip = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Malzemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Malzemeler_Kullanicilar_MusteriId",
                        column: x => x.MusteriId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SatinAlimlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AraciId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerilenCip = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatinAlimlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatinAlimlar_Kullanicilar_AraciId",
                        column: x => x.AraciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SatinAlimlar_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AcikArtirmalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AraciId = table.Column<int>(type: "integer", nullable: false),
                    MalzemeId = table.Column<int>(type: "integer", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<string>(type: "text", nullable: false),
                    KazananFirmaId = table.Column<int>(type: "integer", nullable: true),
                    KazanilanFiyat = table.Column<decimal>(type: "numeric", nullable: true),
                    SatinAlimId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcikArtirmalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcikArtirmalar_Kullanicilar_AraciId",
                        column: x => x.AraciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcikArtirmalar_Kullanicilar_KazananFirmaId",
                        column: x => x.KazananFirmaId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcikArtirmalar_Malzemeler_MalzemeId",
                        column: x => x.MalzemeId,
                        principalTable: "Malzemeler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AcikArtirmalar_SatinAlimlar_SatinAlimId",
                        column: x => x.SatinAlimId,
                        principalTable: "SatinAlimlar",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Teklifler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirmaId = table.Column<int>(type: "integer", nullable: false),
                    AcikArtirmaId = table.Column<int>(type: "integer", nullable: false),
                    TeklifTutar = table.Column<decimal>(type: "numeric", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teklifler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teklifler_AcikArtirmalar_AcikArtirmaId",
                        column: x => x.AcikArtirmaId,
                        principalTable: "AcikArtirmalar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Teklifler_Kullanicilar_FirmaId",
                        column: x => x.FirmaId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcikArtirmalar_AraciId",
                table: "AcikArtirmalar",
                column: "AraciId");

            migrationBuilder.CreateIndex(
                name: "IX_AcikArtirmalar_KazananFirmaId",
                table: "AcikArtirmalar",
                column: "KazananFirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_AcikArtirmalar_MalzemeId",
                table: "AcikArtirmalar",
                column: "MalzemeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AcikArtirmalar_SatinAlimId",
                table: "AcikArtirmalar",
                column: "SatinAlimId");

            migrationBuilder.CreateIndex(
                name: "IX_Malzemeler_MusteriId",
                table: "Malzemeler",
                column: "MusteriId");

            migrationBuilder.CreateIndex(
                name: "IX_SatinAlimlar_AraciId",
                table: "SatinAlimlar",
                column: "AraciId");

            migrationBuilder.CreateIndex(
                name: "IX_SatinAlimlar_MalzemeId",
                table: "SatinAlimlar",
                column: "MalzemeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_AcikArtirmaId",
                table: "Teklifler",
                column: "AcikArtirmaId");

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_FirmaId",
                table: "Teklifler",
                column: "FirmaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Teklifler");

            migrationBuilder.DropTable(
                name: "AcikArtirmalar");

            migrationBuilder.DropTable(
                name: "SatinAlimlar");

            migrationBuilder.DropTable(
                name: "Malzemeler");

            migrationBuilder.DropTable(
                name: "Kullanicilar");
        }
    }
}
