using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddHidNumberToCardsAndRemovedConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CardHolders_TenantId_Email",
                schema: "slk",
                table: "CardHolders");

            migrationBuilder.DropIndex(
                name: "IX_CardHolders_TenantId_UniqueIdentifier",
                schema: "slk",
                table: "CardHolders");

            migrationBuilder.DropIndex(
                name: "IX_CardCredentials_SerialNumber",
                schema: "slk",
                table: "CardCredentials");

            migrationBuilder.AddColumn<string>(
                name: "ServiceTag",
                schema: "slk",
                table: "Lockers",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UniqueIdentifier",
                schema: "slk",
                table: "CardHolders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "slk",
                table: "CardHolders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                schema: "slk",
                table: "CardCredentials",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(16)");

            migrationBuilder.AddColumn<string>(
                name: "CardLabel",
                schema: "slk",
                table: "CardCredentials",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HidNumber",
                schema: "slk",
                table: "CardCredentials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                schema: "slk",
                table: "Tenants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lockers_TenantId_ServiceTag",
                schema: "slk",
                table: "Lockers",
                columns: new[] { "TenantId", "ServiceTag" },
                unique: true,
                filter: "[ServiceTag] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CardHolders_TenantId_Email",
                schema: "slk",
                table: "CardHolders",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CardHolders_TenantId_UniqueIdentifier",
                schema: "slk",
                table: "CardHolders",
                columns: new[] { "TenantId", "UniqueIdentifier" },
                unique: true,
                filter: "[UniqueIdentifier] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CardCredentials_SerialNumber",
                schema: "slk",
                table: "CardCredentials",
                column: "SerialNumber",
                unique: true,
                filter: "[SerialNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_Name",
                schema: "slk",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Lockers_TenantId_ServiceTag",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.DropIndex(
                name: "IX_CardHolders_TenantId_Email",
                schema: "slk",
                table: "CardHolders");

            migrationBuilder.DropIndex(
                name: "IX_CardHolders_TenantId_UniqueIdentifier",
                schema: "slk",
                table: "CardHolders");

            migrationBuilder.DropIndex(
                name: "IX_CardCredentials_SerialNumber",
                schema: "slk",
                table: "CardCredentials");

            migrationBuilder.DropColumn(
                name: "ServiceTag",
                schema: "slk",
                table: "Lockers");

            migrationBuilder.DropColumn(
                name: "CardLabel",
                schema: "slk",
                table: "CardCredentials");

            migrationBuilder.DropColumn(
                name: "HidNumber",
                schema: "slk",
                table: "CardCredentials");

            migrationBuilder.AlterColumn<string>(
                name: "UniqueIdentifier",
                schema: "slk",
                table: "CardHolders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "slk",
                table: "CardHolders",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SerialNumber",
                schema: "slk",
                table: "CardCredentials",
                type: "varchar(16)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardHolders_TenantId_Email",
                schema: "slk",
                table: "CardHolders",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardHolders_TenantId_UniqueIdentifier",
                schema: "slk",
                table: "CardHolders",
                columns: new[] { "TenantId", "UniqueIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardCredentials_SerialNumber",
                schema: "slk",
                table: "CardCredentials",
                column: "SerialNumber",
                unique: true);
        }
    }
}
