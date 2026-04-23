using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenusPos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorregirForeignKeysMovimientoCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Empleados_idEmpleado",
                table: "MovimientosCaja");

            migrationBuilder.RenameColumn(
                name: "idEmpleado",
                table: "MovimientosCaja",
                newName: "IdEmpleado");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosCaja_idEmpleado",
                table: "MovimientosCaja",
                newName: "IX_MovimientosCaja_IdEmpleado");

            migrationBuilder.AddColumn<int>(
                name: "EmpleadoId",
                table: "MovimientosCaja",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosCaja_EmpleadoId",
                table: "MovimientosCaja",
                column: "EmpleadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Empleados_EmpleadoId",
                table: "MovimientosCaja",
                column: "EmpleadoId",
                principalTable: "Empleados",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Empleados_IdEmpleado",
                table: "MovimientosCaja",
                column: "IdEmpleado",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Empleados_EmpleadoId",
                table: "MovimientosCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosCaja_Empleados_IdEmpleado",
                table: "MovimientosCaja");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosCaja_EmpleadoId",
                table: "MovimientosCaja");

            migrationBuilder.DropColumn(
                name: "EmpleadoId",
                table: "MovimientosCaja");

            migrationBuilder.RenameColumn(
                name: "IdEmpleado",
                table: "MovimientosCaja",
                newName: "idEmpleado");

            migrationBuilder.RenameIndex(
                name: "IX_MovimientosCaja_IdEmpleado",
                table: "MovimientosCaja",
                newName: "IX_MovimientosCaja_idEmpleado");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosCaja_Empleados_idEmpleado",
                table: "MovimientosCaja",
                column: "idEmpleado",
                principalTable: "Empleados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
