using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddsKioskAccessCodeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KioskAccessCodes",
                schema: "slk",
                columns: table => new
                {
                    KioskAccessCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasBeenUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CardHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KioskAccessCodes", x => x.KioskAccessCodeId);
                    table.ForeignKey(
                        name: "FK_KioskAccessCodes_CardHolders_CardHolderId",
                        column: x => x.CardHolderId,
                        principalSchema: "slk",
                        principalTable: "CardHolders",
                        principalColumn: "CardHolderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KioskAccessCodes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "slk",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KioskAccessCodes_CardHolderId",
                schema: "slk",
                table: "KioskAccessCodes",
                column: "CardHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_KioskAccessCodes_TenantId",
                schema: "slk",
                table: "KioskAccessCodes",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KioskAccessCodes",
                schema: "slk");
        }
    }
}
