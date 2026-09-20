using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rassef.Migrations
{
    /// <inheritdoc />
    public partial class Add_Caller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CallerId",
                table: "TransferRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DockId",
                table: "TransferRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CallerId",
                table: "SupplierRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DockId",
                table: "SupplierRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderMaintenance",
                table: "Docks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxTruckCount",
                table: "Docks",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_CallerId",
                table: "TransferRequests",
                column: "CallerId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_DockId",
                table: "TransferRequests",
                column: "DockId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRequests_CallerId",
                table: "SupplierRequests",
                column: "CallerId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRequests_DockId",
                table: "SupplierRequests",
                column: "DockId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierRequests_Docks_DockId",
                table: "SupplierRequests",
                column: "DockId",
                principalTable: "Docks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierRequests_Users_CallerId",
                table: "SupplierRequests",
                column: "CallerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferRequests_Docks_DockId",
                table: "TransferRequests",
                column: "DockId",
                principalTable: "Docks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransferRequests_Users_CallerId",
                table: "TransferRequests",
                column: "CallerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierRequests_Docks_DockId",
                table: "SupplierRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierRequests_Users_CallerId",
                table: "SupplierRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferRequests_Docks_DockId",
                table: "TransferRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_TransferRequests_Users_CallerId",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_TransferRequests_CallerId",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_TransferRequests_DockId",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupplierRequests_CallerId",
                table: "SupplierRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupplierRequests_DockId",
                table: "SupplierRequests");

            migrationBuilder.DropColumn(
                name: "CallerId",
                table: "TransferRequests");

            migrationBuilder.DropColumn(
                name: "DockId",
                table: "TransferRequests");

            migrationBuilder.DropColumn(
                name: "CallerId",
                table: "SupplierRequests");

            migrationBuilder.DropColumn(
                name: "DockId",
                table: "SupplierRequests");

            migrationBuilder.DropColumn(
                name: "IsUnderMaintenance",
                table: "Docks");

            migrationBuilder.DropColumn(
                name: "MaxTruckCount",
                table: "Docks");
        }
    }
}
