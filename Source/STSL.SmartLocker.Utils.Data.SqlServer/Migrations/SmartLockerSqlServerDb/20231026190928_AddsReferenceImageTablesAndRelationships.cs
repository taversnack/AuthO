using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class AddsReferenceImageTablesAndRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UniqueIdentifierAlias",
                schema: "slk",
                table: "Tenants",
                newName: "CardHolderUniqueIdentifierAlias");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentReferenceImageId",
                schema: "slk",
                table: "LockerBanks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentReferenceImageId",
                schema: "slk",
                table: "Locations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocationReferenceImages",
                schema: "slk",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MetaData_FileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    MetaData_AzureBlobName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaData_PixelWidth = table.Column<int>(type: "int", nullable: false),
                    MetaData_PixelHeight = table.Column<int>(type: "int", nullable: false),
                    MetaData_ByteSize = table.Column<int>(type: "int", nullable: false),
                    MetaData_MimeType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MetaData_UploadedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MetaData_UploadedByCardHolderEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationReferenceImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationReferenceImages_Locations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "slk",
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LocationReferenceImages_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "slk",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LockerBankReferenceImages",
                schema: "slk",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockerBankId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MetaData_FileName = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    MetaData_AzureBlobName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaData_PixelWidth = table.Column<int>(type: "int", nullable: false),
                    MetaData_PixelHeight = table.Column<int>(type: "int", nullable: false),
                    MetaData_ByteSize = table.Column<int>(type: "int", nullable: false),
                    MetaData_MimeType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    MetaData_UploadedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MetaData_UploadedByCardHolderEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerBankReferenceImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LockerBankReferenceImages_LockerBanks_LockerBankId",
                        column: x => x.LockerBankId,
                        principalSchema: "slk",
                        principalTable: "LockerBanks",
                        principalColumn: "LockerBankId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LockerBankReferenceImages_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalSchema: "slk",
                        principalTable: "Tenants",
                        principalColumn: "TenantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LockerBanks_CurrentReferenceImageId",
                schema: "slk",
                table: "LockerBanks",
                column: "CurrentReferenceImageId",
                unique: true,
                filter: "[CurrentReferenceImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CurrentReferenceImageId",
                schema: "slk",
                table: "Locations",
                column: "CurrentReferenceImageId",
                unique: true,
                filter: "[CurrentReferenceImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LocationReferenceImages_LocationId",
                schema: "slk",
                table: "LocationReferenceImages",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationReferenceImages_TenantId",
                schema: "slk",
                table: "LocationReferenceImages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankReferenceImages_LockerBankId",
                schema: "slk",
                table: "LockerBankReferenceImages",
                column: "LockerBankId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankReferenceImages_TenantId",
                schema: "slk",
                table: "LockerBankReferenceImages",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_LocationReferenceImages_CurrentReferenceImageId",
                schema: "slk",
                table: "Locations",
                column: "CurrentReferenceImageId",
                principalSchema: "slk",
                principalTable: "LocationReferenceImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LockerBanks_LockerBankReferenceImages_CurrentReferenceImageId",
                schema: "slk",
                table: "LockerBanks",
                column: "CurrentReferenceImageId",
                principalSchema: "slk",
                principalTable: "LockerBankReferenceImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_LocationReferenceImages_CurrentReferenceImageId",
                schema: "slk",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_LockerBanks_LockerBankReferenceImages_CurrentReferenceImageId",
                schema: "slk",
                table: "LockerBanks");

            migrationBuilder.DropTable(
                name: "LocationReferenceImages",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "LockerBankReferenceImages",
                schema: "slk");

            migrationBuilder.DropIndex(
                name: "IX_LockerBanks_CurrentReferenceImageId",
                schema: "slk",
                table: "LockerBanks");

            migrationBuilder.DropIndex(
                name: "IX_Locations_CurrentReferenceImageId",
                schema: "slk",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CurrentReferenceImageId",
                schema: "slk",
                table: "LockerBanks");

            migrationBuilder.DropColumn(
                name: "CurrentReferenceImageId",
                schema: "slk",
                table: "Locations");

            migrationBuilder.RenameColumn(
                name: "CardHolderUniqueIdentifierAlias",
                schema: "slk",
                table: "Tenants",
                newName: "UniqueIdentifierAlias");
        }
    }
}
