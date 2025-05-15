using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBM.Migrations
{
    /// <inheritdoc />
    public partial class teklif_firmaid_only : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teklifler_Kullanicilar_KullaniciId",
                table: "Teklifler");

            migrationBuilder.DropIndex(
                name: "IX_Teklifler_KullaniciId",
                table: "Teklifler");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Teklifler");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KullaniciId",
                table: "Teklifler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Teklifler_KullaniciId",
                table: "Teklifler",
                column: "KullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teklifler_Kullanicilar_KullaniciId",
                table: "Teklifler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
