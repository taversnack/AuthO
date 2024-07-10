using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddLocationtoKiosk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Kiosks_LocationId",
                schema: "slk",
                table: "Kiosks",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Kiosks_Locations_LocationId",
                schema: "slk",
                table: "Kiosks",
                column: "LocationId",
                principalSchema: "slk",
                principalTable: "Locations",
                principalColumn: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kiosks_Locations_LocationId",
                schema: "slk",
                table: "Kiosks");

            migrationBuilder.DropIndex(
                name: "IX_Kiosks_LocationId",
                schema: "slk",
                table: "Kiosks");
        }
    }
}
