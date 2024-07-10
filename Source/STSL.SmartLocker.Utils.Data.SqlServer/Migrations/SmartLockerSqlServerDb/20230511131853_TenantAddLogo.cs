using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class TenantAddLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Logo",
                schema: "slk",
                table: "Tenants",
                type: "varbinary(max)",
                maxLength: 2097152,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoMimeType",
                schema: "slk",
                table: "Tenants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Logo",
                schema: "slk",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "LogoMimeType",
                schema: "slk",
                table: "Tenants");
        }
    }
}
