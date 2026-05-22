using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenusPos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechasInactivoEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InactivoDesde",
                table: "Empleados",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "InactivoHasta",
                table: "Empleados",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InactivoDesde",
                table: "Empleados");

            migrationBuilder.DropColumn(
                name: "InactivoHasta",
                table: "Empleados");
        }
    }
}
