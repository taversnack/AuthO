using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddLockerLeaseHolders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locks_Lockers_LockerId",
                schema: "slk",
                table: "Locks");

            migrationBuilder.DropColumn(
                name: "HasTenant",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "AbsoluteLeaseExpiry",
                schema: "slk",
                table: "Lockers",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CardHolderId",
                schema: "slk",
                table: "Lockers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentLeaseId",
                schema: "slk",
                table: "Lockers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LockerLeases",
                schema: "slk",
                columns: table => new
                {
                    LockerLeaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockerBankBehaviour = table.Column<int>(type: "int", nullable: false),
                    CardCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CardHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LockerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LockId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerLeases", x => x.LockerLeaseId);
                    table.ForeignKey(
                        name: "FK_LockerLeases_CardCredentials_CardCredentialId",
                        column: x => x.CardCredentialId,
                        principalSchema: "slk",
                        principalTable: "CardCredentials",
                        principalColumn: "CardCredentialId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LockerLeases_CardHolders_CardHolderId",
                        column: x => x.CardHolderId,
                        principalSchema: "slk",
                        principalTable: "CardHolders",
                        principalColumn: "CardHolderId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LockerLeases_Lockers_LockerId",
                        column: x => x.LockerId,
                        principalSchema: "slk",
                        principalTable: "Lockers",
                        principalColumn: "LockerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LockerLeases_Locks_LockId",
                        column: x => x.LockId,
                        principalSchema: "slk",
                        principalTable: "Locks",
                        principalColumn: "LockId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LockerLeases_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "slk",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lockers_CardHolderId",
                schema: "slk",
                table: "Lockers",
                column: "CardHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Lockers_CurrentLeaseId",
                schema: "slk",
                table: "Lockers",
                column: "CurrentLeaseId",
                unique: true,
                filter: "[CurrentLeaseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LockerLeases_CardCredentialId",
                schema: "slk",
                table: "LockerLeases",
                column: "CardCredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerLeases_CardHolderId",
                schema: "slk",
                table: "LockerLeases",
                column: "CardHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerLeases_LockerId",
                schema: "slk",
                table: "LockerLeases",
                column: "LockerId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerLeases_LockId",
                schema: "slk",
                table: "LockerLeases",
                column: "LockId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerLeases_TenantId",
                schema: "slk",
                table: "LockerLeases",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lockers_CardHolders_CardHolderId",
                schema: "slk",
                table: "Lockers",
                column: "CardHolderId",
                principalSchema: "slk",
                principalTable: "CardHolders",
                principalColumn: "CardHolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lockers_LockerLeases_CurrentLeaseId",
                schema: "slk",
                table: "Lockers",
                column: "CurrentLeaseId",
                principalSchema: "slk",
                principalTable: "LockerLeases",
                principalColumn: "LockerLeaseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Locks_Lockers_LockerId",
                schema: "slk",
                table: "Locks",
                column: "LockerId",
                principalSchema: "slk",
                principalTable: "Lockers",
                principalColumn: "LockerId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lockers_CardHolders_CardHolderId",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.DropForeignKey(
                name: "FK_Lockers_LockerLeases_CurrentLeaseId",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.DropForeignKey(
                name: "FK_Locks_Lockers_LockerId",
                schema: "slk",
                table: "Locks");

            migrationBuilder.DropTable(
                name: "LockerLeases",
                schema: "slk");

            migrationBuilder.DropIndex(
                name: "IX_Lockers_CardHolderId",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.DropIndex(
                name: "IX_Lockers_CurrentLeaseId",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "CardHolderId",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "CurrentLeaseId",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AbsoluteLeaseExpiry",
                schema: "slk",
                table: "Lockers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasTenant",
                schema: "slk",
                table: "Lockers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Locks_Lockers_LockerId",
                schema: "slk",
                table: "Locks",
                column: "LockerId",
                principalSchema: "slk",
                principalTable: "Lockers",
                principalColumn: "LockerId");
        }
    }
}
