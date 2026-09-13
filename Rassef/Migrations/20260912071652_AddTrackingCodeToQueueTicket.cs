using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rassef.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingCodeToQueueTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TrackingCode",
                table: "QueueTickets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
            // NEW: ملء كل صف قديم بكود عشوائي مختلف قبل ما نعمل الـ Unique Index
            migrationBuilder.Sql(
                "UPDATE dbo.QueueTickets SET TrackingCode = NEWID() " +
                "WHERE TrackingCode = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTickets_TrackingCode",
                table: "QueueTickets",
                column: "TrackingCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QueueTickets_TrackingCode",
                table: "QueueTickets");

            migrationBuilder.DropColumn(
                name: "TrackingCode",
                table: "QueueTickets");
        }
    }
}
