using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenusPos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RediseñoCajaVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Caja_Reservas_IdReserva",
                table: "Caja");

            migrationBuilder.DropIndex(
                name: "IX_Caja_IdReserva",
                table: "Caja");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Caja");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Caja");

            migrationBuilder.DropColumn(
                name: "IdReserva",
                table: "Caja");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCierre",
                table: "Caja",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Caja",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCaja = table.Column<int>(type: "int", nullable: false),
                    IdReserva = table.Column<int>(type: "int", nullable: false),
                    IdCliente = table.Column<int>(type: "int", nullable: false),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaVenta = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Caja_IdCaja",
                        column: x => x.IdCaja,
                        principalTable: "Caja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Empleados_IdEmpleado",
                        column: x => x.IdEmpleado,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Reservas_IdReserva",
                        column: x => x.IdReserva,
                        principalTable: "Reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentaDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdVenta = table.Column<int>(type: "int", nullable: false),
                    IdServicio = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Servicios_IdServicio",
                        column: x => x.IdServicio,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Ventas_IdVenta",
                        column: x => x.IdVenta,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_IdServicio",
                table: "VentaDetalles",
                column: "IdServicio");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_IdVenta",
                table: "VentaDetalles",
                column: "IdVenta");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdCaja",
                table: "Ventas",
                column: "IdCaja");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdCliente",
                table: "Ventas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdEmpleado",
                table: "Ventas",
                column: "IdEmpleado");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_IdReserva",
                table: "Ventas",
                column: "IdReserva");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VentaDetalles");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Caja");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaCierre",
                table: "Caja",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Caja",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Caja",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IdReserva",
                table: "Caja",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Caja_IdReserva",
                table: "Caja",
                column: "IdReserva");

            migrationBuilder.AddForeignKey(
                name: "FK_Caja_Reservas_IdReserva",
                table: "Caja",
                column: "IdReserva",
                principalTable: "Reservas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
