using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenusPos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RediseñoCajaVenta2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalEfectivo",
                table: "Caja",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalTarjeta",
                table: "Caja",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalTransferencia",
                table: "Caja",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalVentas",
                table: "Caja",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalEfectivo",
                table: "Caja");

            migrationBuilder.DropColumn(
                name: "TotalTarjeta",
                table: "Caja");

            migrationBuilder.DropColumn(
                name: "TotalTransferencia",
                table: "Caja");

            migrationBuilder.DropColumn(
                name: "TotalVentas",
                table: "Caja");
        }
    }
}
