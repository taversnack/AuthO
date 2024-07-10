using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddsUniqueIdentifierAliasToTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqueIdentifierAlias",
                schema: "slk",
                table: "Tenants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "Unique Identifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniqueIdentifierAlias",
                schema: "slk",
                table: "Tenants");
        }
    }
}
