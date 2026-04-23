using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenusPos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModuloReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Reservas",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CodigoReserva",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DuracionMinutos",
                table: "Reservas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioTotal",
                table: "Reservas",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ConfiguracionesPrecios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesPrecios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservaServicios",
                columns: table => new
                {
                    IdReserva = table.Column<int>(type: "int", nullable: false),
                    IdServicio = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservaServicios", x => new { x.IdReserva, x.IdServicio });
                    table.ForeignKey(
                        name: "FK_ReservaServicios_Reservas_IdReserva",
                        column: x => x.IdReserva,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReservaServicios_Servicios_IdServicio",
                        column: x => x.IdServicio,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_CodigoReserva",
                table: "Reservas",
                column: "CodigoReserva",
                unique: true,
                filter: "[CodigoReserva] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_FechaReserva_IdEmpleado",
                table: "Reservas",
                columns: new[] { "FechaReserva", "IdEmpleado" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesPrecios_Clave",
                table: "ConfiguracionesPrecios",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservaServicios_IdServicio",
                table: "ReservaServicios",
                column: "IdServicio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionesPrecios");

            migrationBuilder.DropTable(
                name: "ReservaServicios");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_CodigoReserva",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_FechaReserva_IdEmpleado",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "CodigoReserva",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "DuracionMinutos",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "PrecioTotal",
                table: "Reservas");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Reservas",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
