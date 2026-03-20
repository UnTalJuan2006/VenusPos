using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VenusPos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Empleados",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Empleados");
        }
    }
}
