using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddTenantForeignKeysToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_CardCredentials_Tenants_TenantId",
                schema: "slk",
                table: "CardCredentials",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CardHolders_Tenants_TenantId",
                schema: "slk",
                table: "CardHolders",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Tenants_TenantId",
                schema: "slk",
                table: "Locations",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LockerBankAdmins_Tenants_TenantId",
                schema: "slk",
                table: "LockerBankAdmins",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LockerBankCardCredentials_Tenants_TenantId",
                schema: "slk",
                table: "LockerBankCardCredentials",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LockerBanks_Tenants_TenantId",
                schema: "slk",
                table: "LockerBanks",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LockerCardCredentials_Tenants_TenantId",
                schema: "slk",
                table: "LockerCardCredentials",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lockers_Tenants_TenantId",
                schema: "slk",
                table: "Lockers",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Locks_Tenants_TenantId",
                schema: "slk",
                table: "Locks",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CardCredentials_Tenants_TenantId",
                schema: "slk",
                table: "CardCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_CardHolders_Tenants_TenantId",
                schema: "slk",
                table: "CardHolders");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Tenants_TenantId",
                schema: "slk",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_LockerBankAdmins_Tenants_TenantId",
                schema: "slk",
                table: "LockerBankAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_LockerBankCardCredentials_Tenants_TenantId",
                schema: "slk",
                table: "LockerBankCardCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_LockerBanks_Tenants_TenantId",
                schema: "slk",
                table: "LockerBanks");

            migrationBuilder.DropForeignKey(
                name: "FK_LockerCardCredentials_Tenants_TenantId",
                schema: "slk",
                table: "LockerCardCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_Lockers_Tenants_TenantId",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.DropForeignKey(
                name: "FK_Locks_Tenants_TenantId",
                schema: "slk",
                table: "Locks");
        }
    }
}
