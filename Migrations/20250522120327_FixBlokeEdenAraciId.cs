using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBM.Migrations
{
    /// <inheritdoc />
    public partial class FixBlokeEdenAraciId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
 

 

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
            
        }
    }
}
