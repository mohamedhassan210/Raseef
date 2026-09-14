using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rassef.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------- Warehouses.Id (identity) / Warehouses.CreatedById ----------
            // REMOVED — confirmed against the live DB (SSMS) that this is already in
            // place: Id is already IDENTITY(1,1), CreatedById already exists with
            // FK_Warehouses_Users_CreatedById + IX_Warehouses_CreatedById already
            // present, and the old FK_Warehouses_Users_Id (shared-PK) is already gone.
            // Applied outside of EF's tracked migration history at some earlier point;
            // redoing it here is what caused the "column needs to be dropped and
            // recreated" error. Nothing to do — the model snapshot already matches
            // reality, so future add-migration calls will diff correctly from here.

            // ---------- Users.LastPickedWarehouseId (nullable — safe to add as-is) ----------
            migrationBuilder.AddColumn<int>(
                name: "LastPickedWarehouseId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastPickedWarehouseId",
                table: "Users",
                column: "LastPickedWarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Warehouses_LastPickedWarehouseId",
                table: "Users",
                column: "LastPickedWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ---------- UserWarehouses (brand-new, empty table — safe to add as-is) ----------
            migrationBuilder.CreateTable(
                name: "UserWarehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    CreatedAT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAT = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWarehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWarehouses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserWarehouses_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWarehouses_UserId",
                table: "UserWarehouses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWarehouses_WarehouseId",
                table: "UserWarehouses",
                column: "WarehouseId");

            // ---------- Shifts.WarehouseId ----------
            // Test data only, per instruction — clear the two Restrict-linked dependents
            // first (QueueSettings/QueueTickets reference Shifts and would otherwise block
            // the delete below even though their own ShiftId columns are nullable), then
            // clear Shifts itself so the new required column can be added with no backfill
            // value needed at all.
            migrationBuilder.Sql("DELETE FROM QueueTickets WHERE ShiftId IS NOT NULL;");
            migrationBuilder.Sql("DELETE FROM QueueSettings WHERE ShiftId IS NOT NULL;");
            migrationBuilder.Sql("DELETE FROM Shifts;");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Shifts",
                type: "int",
                nullable: false,
                defaultValue: 0); // irrelevant — table is empty at this point, nothing to backfill

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_WarehouseId",
                table: "Shifts",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Warehouses_WarehouseId",
                table: "Shifts",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Warehouses_WarehouseId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_WarehouseId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Shifts");
            // Note: the Shifts/QueueSettings/QueueTickets rows deleted in Up() are gone for
            // good — this migration is not reversible back to that data, only to the schema.

            migrationBuilder.DropForeignKey(
                name: "FK_UserWarehouses_Users_UserId",
                table: "UserWarehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserWarehouses_Warehouses_WarehouseId",
                table: "UserWarehouses");

            migrationBuilder.DropTable(
                name: "UserWarehouses");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Warehouses_LastPickedWarehouseId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastPickedWarehouseId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastPickedWarehouseId",
                table: "Users");

            // ---------- Warehouses.Id / CreatedById ----------
            // REMOVED — matches Up(): this migration never touches Warehouses.Id or
            // CreatedById (they were already correct in the live DB before this
            // migration existed), so Down() must not try to revert them either.
        }
    }
}