using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rassef.Migrations
{
    /// <inheritdoc />
    public partial class FinalInShaaAllah2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                table: "QueueTickets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedAT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QueueSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResetType = table.Column<int>(type: "int", nullable: false),
                    ShiftId = table.Column<int>(type: "int", nullable: true),
                    CreatedAT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueueSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueueSettings_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueueTickets_ShiftId",
                table: "QueueTickets",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueSettings_ShiftId",
                table: "QueueSettings",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueueTickets_Shifts_ShiftId",
                table: "QueueTickets",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueueTickets_Shifts_ShiftId",
                table: "QueueTickets");

            migrationBuilder.DropTable(
                name: "QueueSettings");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_QueueTickets_ShiftId",
                table: "QueueTickets");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "QueueTickets");
        }
    }
}
