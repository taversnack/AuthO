using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class ShiftLockerChangesAndOtherMinorUpgrades : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "LockerBankCardCredentials", 
                schema: "slk", 
                newName: "LockerBankSpecialCardCredentials", 
                newSchema: "slk");

            migrationBuilder.RenameColumn(
                name: "CardHolderAlias",
                schema: "slk",
                table: "Tenants",
                newName: "CardHolderAliasSingular");

            migrationBuilder.AddColumn<string>(
                name: "CardHolderAliasPlural",
                schema: "slk",
                table: "Tenants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HelpPortalUrl",
                schema: "slk",
                table: "Tenants",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SecurityType",
                schema: "slk",
                table: "Lockers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LockerBankLeaseUsers",
                schema: "slk",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockerBankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerBankLeaseUsers", x => new { x.TenantId, x.LockerBankId, x.CardHolderId });
                    table.ForeignKey(
                        name: "FK_LockerBankLeaseUsers_CardHolders_CardHolderId",
                        column: x => x.CardHolderId,
                        principalSchema: "slk",
                        principalTable: "CardHolders",
                        principalColumn: "CardHolderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerBankLeaseUsers_LockerBanks_LockerBankId",
                        column: x => x.LockerBankId,
                        principalSchema: "slk",
                        principalTable: "LockerBanks",
                        principalColumn: "LockerBankId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerBankLeaseUsers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "slk",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LockerBankUserCardCredentials",
                schema: "slk",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockerBankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerBankUserCardCredentials", x => new { x.TenantId, x.LockerBankId, x.CardCredentialId });
                    table.ForeignKey(
                        name: "FK_LockerBankUserCardCredentials_CardCredentials_CardCredentialId",
                        column: x => x.CardCredentialId,
                        principalSchema: "slk",
                        principalTable: "CardCredentials",
                        principalColumn: "CardCredentialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerBankUserCardCredentials_LockerBanks_LockerBankId",
                        column: x => x.LockerBankId,
                        principalSchema: "slk",
                        principalTable: "LockerBanks",
                        principalColumn: "LockerBankId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerBankUserCardCredentials_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "slk",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LockerOwners",
                schema: "slk",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerOwners", x => new { x.TenantId, x.LockerId, x.CardHolderId });
                    table.ForeignKey(
                        name: "FK_LockerOwners_CardHolders_CardHolderId",
                        column: x => x.CardHolderId,
                        principalSchema: "slk",
                        principalTable: "CardHolders",
                        principalColumn: "CardHolderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerOwners_Lockers_LockerId",
                        column: x => x.LockerId,
                        principalSchema: "slk",
                        principalTable: "Lockers",
                        principalColumn: "LockerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerOwners_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "slk",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankLeaseUsers_CardHolderId",
                schema: "slk",
                table: "LockerBankLeaseUsers",
                column: "CardHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankLeaseUsers_LockerBankId",
                schema: "slk",
                table: "LockerBankLeaseUsers",
                column: "LockerBankId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankLeaseUsers_TenantId",
                schema: "slk",
                table: "LockerBankLeaseUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankUserCardCredentials_CardCredentialId",
                schema: "slk",
                table: "LockerBankUserCardCredentials",
                column: "CardCredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankUserCardCredentials_LockerBankId",
                schema: "slk",
                table: "LockerBankUserCardCredentials",
                column: "LockerBankId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankUserCardCredentials_TenantId",
                schema: "slk",
                table: "LockerBankUserCardCredentials",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerOwners_CardHolderId",
                schema: "slk",
                table: "LockerOwners",
                column: "CardHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerOwners_LockerId",
                schema: "slk",
                table: "LockerOwners",
                column: "LockerId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerOwners_TenantId",
                schema: "slk",
                table: "LockerOwners",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LockerBankLeaseUsers",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "LockerBankUserCardCredentials",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "LockerOwners",
                schema: "slk");

            migrationBuilder.DropColumn(
                name: "CardHolderAliasPlural",
                schema: "slk",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "HelpPortalUrl",
                schema: "slk",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "SecurityType",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.RenameColumn(
                name: "CardHolderAliasSingular",
                schema: "slk",
                table: "Tenants",
                newName: "CardHolderAlias");

            migrationBuilder.RenameTable(
                name: "LockerBankSpecialCardCredentials",
                schema: "slk",
                newName: "LockerBankCardCredentials",
                newSchema: "slk");
        }
    }
}
