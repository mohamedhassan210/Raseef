using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rassef.Migrations
{
    /// <inheritdoc />
    public partial class MakeDriverTypeCodeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Code",
                table: "DriverTypes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DriverTypes_Code",
                table: "DriverTypes",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DriverTypes_Code",
                table: "DriverTypes");

            migrationBuilder.AlterColumn<int>(
                name: "Code",
                table: "DriverTypes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
