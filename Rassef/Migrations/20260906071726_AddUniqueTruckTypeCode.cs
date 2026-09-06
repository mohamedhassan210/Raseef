using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rassef.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTruckTypeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransferRequests_RequestStatusId",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupplierRequests_RequestStatusId",
                table: "SupplierRequests");

            migrationBuilder.DropIndex(
                name: "IX_QueueTickets_TicketStatusId",
                table: "QueueTickets");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Users",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "TruckTypeCode",
                table: "TruckTypes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "PlateLetter",
                table: "Trucks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "QueueTickets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NationalId",
                table: "Users",
                column: "NationalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TruckTypes_TruckTypeCode",
                table: "TruckTypes",
                column: "TruckTypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_PlateNumber_PlateLetter",
                table: "Trucks",
                columns: new[] { "PlateNumber", "PlateLetter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_AvizNumber",
                table: "TransferRequests",
                column: "AvizNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_PermitNumber",
                table: "TransferRequests",
                column: "PermitNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_RequestStatusId_DepartmentId_CreatedAT",
                table: "TransferRequests",
                columns: new[] { "RequestStatusId", "DepartmentId", "CreatedAT" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRequests_PermitNumber",
                table: "SupplierRequests",
                column: "PermitNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRequests_RequestStatusId_DepartmentId_CreatedAT",
                table: "SupplierRequests",
                columns: new[] { "RequestStatusId", "DepartmentId", "CreatedAT" });

            migrationBuilder.CreateIndex(
                name: "IX_QueueTickets_CreatedAT",
                table: "QueueTickets",
                column: "CreatedAT");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTickets_TicketNumber",
                table: "QueueTickets",
                column: "TicketNumber");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTickets_TicketStatusId_DepartmentId_QueueTime",
                table: "QueueTickets",
                columns: new[] { "TicketStatusId", "DepartmentId", "QueueTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_NationalId",
                table: "Drivers",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_Phone",
                table: "Drivers",
                column: "Phone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_NationalId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TruckTypes_TruckTypeCode",
                table: "TruckTypes");

            migrationBuilder.DropIndex(
                name: "IX_Trucks_PlateNumber_PlateLetter",
                table: "Trucks");

            migrationBuilder.DropIndex(
                name: "IX_TransferRequests_AvizNumber",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_TransferRequests_PermitNumber",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_TransferRequests_RequestStatusId_DepartmentId_CreatedAT",
                table: "TransferRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupplierRequests_PermitNumber",
                table: "SupplierRequests");

            migrationBuilder.DropIndex(
                name: "IX_SupplierRequests_RequestStatusId_DepartmentId_CreatedAT",
                table: "SupplierRequests");

            migrationBuilder.DropIndex(
                name: "IX_QueueTickets_CreatedAT",
                table: "QueueTickets");

            migrationBuilder.DropIndex(
                name: "IX_QueueTickets_TicketNumber",
                table: "QueueTickets");

            migrationBuilder.DropIndex(
                name: "IX_QueueTickets_TicketStatusId_DepartmentId_QueueTime",
                table: "QueueTickets");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_NationalId",
                table: "Drivers");

            migrationBuilder.DropIndex(
                name: "IX_Drivers_Phone",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "TruckTypeCode",
                table: "TruckTypes");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(14)",
                oldMaxLength: 14);

            migrationBuilder.AlterColumn<string>(
                name: "PlateLetter",
                table: "Trucks",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "TicketNumber",
                table: "QueueTickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_TransferRequests_RequestStatusId",
                table: "TransferRequests",
                column: "RequestStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierRequests_RequestStatusId",
                table: "SupplierRequests",
                column: "RequestStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_QueueTickets_TicketStatusId",
                table: "QueueTickets",
                column: "TicketStatusId");
        }
    }
}
