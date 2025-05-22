using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBM.Migrations
{
    /// <inheritdoc />
    public partial class AddBlokeEdilmeTarihiToMalzeme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BlokeEdilmeTarihi",
                table: "Malzemeler",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlokeEdilmeTarihi",
                table: "Malzemeler");
        }
    }
}
