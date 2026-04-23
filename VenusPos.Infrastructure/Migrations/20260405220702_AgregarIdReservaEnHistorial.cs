using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenusPos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIdReservaEnHistorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdReserva",
                table: "Historiales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Historiales_IdReserva",
                table: "Historiales",
                column: "IdReserva");

            migrationBuilder.AddForeignKey(
                name: "FK_Historiales_Reservas_IdReserva",
                table: "Historiales",
                column: "IdReserva",
                principalTable: "Reservas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Historiales_Reservas_IdReserva",
                table: "Historiales");

            migrationBuilder.DropIndex(
                name: "IX_Historiales_IdReserva",
                table: "Historiales");

            migrationBuilder.DropColumn(
                name: "IdReserva",
                table: "Historiales");
        }
    }
}
