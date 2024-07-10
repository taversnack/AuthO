using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddsTerminationFieldsToCardHolder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KioskAccessCodes_Tenants_TenantId",
                schema: "slk",
                table: "KioskAccessCodes");

            migrationBuilder.AddColumn<bool>(
                name: "IsTerminated",
                schema: "slk",
                table: "CardHolders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminationDate",
                schema: "slk",
                table: "CardHolders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_KioskAccessCodes_Tenants_TenantId",
                schema: "slk",
                table: "KioskAccessCodes",
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
                name: "FK_KioskAccessCodes_Tenants_TenantId",
                schema: "slk",
                table: "KioskAccessCodes");

            migrationBuilder.DropColumn(
                name: "IsTerminated",
                schema: "slk",
                table: "CardHolders");

            migrationBuilder.DropColumn(
                name: "TerminationDate",
                schema: "slk",
                table: "CardHolders");

            migrationBuilder.AddForeignKey(
                name: "FK_KioskAccessCodes_Tenants_TenantId",
                schema: "slk",
                table: "KioskAccessCodes",
                column: "TenantId",
                principalSchema: "slk",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
