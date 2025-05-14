using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBM.Migrations
{
    /// <inheritdoc />
    public partial class MalzemeyeBlokeEdenAraciEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BlokeEdenAraciId",
                table: "Malzemeler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Malzemeler_BlokeEdenAraciId",
                table: "Malzemeler",
                column: "BlokeEdenAraciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Malzemeler_Kullanicilar_BlokeEdenAraciId",
                table: "Malzemeler",
                column: "BlokeEdenAraciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Malzemeler_Kullanicilar_BlokeEdenAraciId",
                table: "Malzemeler");

            migrationBuilder.DropIndex(
                name: "IX_Malzemeler_BlokeEdenAraciId",
                table: "Malzemeler");

            migrationBuilder.DropColumn(
                name: "BlokeEdenAraciId",
                table: "Malzemeler");
        }
    }
}
