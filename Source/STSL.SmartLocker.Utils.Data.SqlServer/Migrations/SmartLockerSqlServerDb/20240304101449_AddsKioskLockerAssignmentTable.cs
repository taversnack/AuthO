using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddsKioskLockerAssignmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KioskLockerAssignments",
                schema: "slk",
                columns: table => new
                {
                    KioskLockerAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CardHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TemporaryCardCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReplacedCardCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignmentDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsTemporaryCardActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskLockerAssignments", x => x.KioskLockerAssignmentId);
                    table.ForeignKey(
                        name: "FK_KioskLockerAssignments_CardCredentials_ReplacedCardCredentialId",
                        column: x => x.ReplacedCardCredentialId,
                        principalSchema: "slk",
                        principalTable: "CardCredentials",
                        principalColumn: "CardCredentialId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KioskLockerAssignments_CardHolders_CardHolderId",
                        column: x => x.CardHolderId,
                        principalSchema: "slk",
                        principalTable: "CardHolders",
                        principalColumn: "CardHolderId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KioskLockerAssignments_Lockers_LockerId",
                        column: x => x.LockerId,
                        principalSchema: "slk",
                        principalTable: "Lockers",
                        principalColumn: "LockerId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KioskLockerAssignments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "slk",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KioskLockerAssignments_CardHolderId",
                schema: "slk",
                table: "KioskLockerAssignments",
                column: "CardHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskLockerAssignments_LockerId",
                schema: "slk",
                table: "KioskLockerAssignments",
                column: "LockerId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskLockerAssignments_ReplacedCardCredentialId",
                schema: "slk",
                table: "KioskLockerAssignments",
                column: "ReplacedCardCredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskLockerAssignments_TenantId",
                schema: "slk",
                table: "KioskLockerAssignments",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KioskLockerAssignments",
                schema: "slk");
        }
    }
}
