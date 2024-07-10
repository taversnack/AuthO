using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "slk");

            migrationBuilder.CreateTable(
                name: "CardHolders",
                schema: "slk",
                columns: table => new
                {
                    CardHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UniqueIdentifier = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardHolders", x => x.CardHolderId);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                schema: "slk",
                columns: table => new
                {
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationId);
                });

            migrationBuilder.CreateTable(
                name: "StringifiedLockerBankBehaviour",
                schema: "slk",
                columns: table => new
                {
                    Value = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringifiedLockerBankBehaviour", x => x.Value);
                });

            migrationBuilder.CreateTable(
                name: "StringifiedLockOperatingMode",
                schema: "slk",
                columns: table => new
                {
                    Value = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StringifiedLockOperatingMode", x => x.Value);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: "slk",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CardHolderAlias = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateTable(
                name: "CardCredentials",
                schema: "slk",
                columns: table => new
                {
                    CardCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(16)", nullable: false),
                    CardType = table.Column<int>(type: "int", nullable: false),
                    CardHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardCredentials", x => x.CardCredentialId);
                    table.ForeignKey(
                        name: "FK_CardCredentials_CardHolders_CardHolderId",
                        column: x => x.CardHolderId,
                        principalSchema: "slk",
                        principalTable: "CardHolders",
                        principalColumn: "CardHolderId");
                });

            migrationBuilder.CreateTable(
                name: "LockerBanks",
                schema: "slk",
                columns: table => new
                {
                    LockerBankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Behaviour = table.Column<int>(type: "int", nullable: false),
                    DefaultLeaseDuration = table.Column<TimeSpan>(type: "time", nullable: true),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerBanks", x => x.LockerBankId);
                    table.ForeignKey(
                        name: "FK_LockerBanks_Locations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "slk",
                        principalTable: "Locations",
                        principalColumn: "LocationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LockerBankAdmins",
                schema: "slk",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockerBankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardHolderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerBankAdmins", x => new { x.TenantId, x.LockerBankId, x.CardHolderId });
                    table.ForeignKey(
                        name: "FK_LockerBankAdmins_CardHolders_CardHolderId",
                        column: x => x.CardHolderId,
                        principalSchema: "slk",
                        principalTable: "CardHolders",
                        principalColumn: "CardHolderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerBankAdmins_LockerBanks_LockerBankId",
                        column: x => x.LockerBankId,
                        principalSchema: "slk",
                        principalTable: "LockerBanks",
                        principalColumn: "LockerBankId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LockerBankCardCredentials",
                schema: "slk",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockerBankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerBankCardCredentials", x => new { x.TenantId, x.LockerBankId, x.CardCredentialId });
                    table.ForeignKey(
                        name: "FK_LockerBankCardCredentials_CardCredentials_CardCredentialId",
                        column: x => x.CardCredentialId,
                        principalSchema: "slk",
                        principalTable: "CardCredentials",
                        principalColumn: "CardCredentialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerBankCardCredentials_LockerBanks_LockerBankId",
                        column: x => x.LockerBankId,
                        principalSchema: "slk",
                        principalTable: "LockerBanks",
                        principalColumn: "LockerBankId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lockers",
                schema: "slk",
                columns: table => new
                {
                    LockerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    HasTenant = table.Column<bool>(type: "bit", nullable: false),
                    AbsoluteLeaseExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LockerBankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lockers", x => x.LockerId);
                    table.ForeignKey(
                        name: "FK_Lockers_LockerBanks_LockerBankId",
                        column: x => x.LockerBankId,
                        principalSchema: "slk",
                        principalTable: "LockerBanks",
                        principalColumn: "LockerBankId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LockerCardCredentials",
                schema: "slk",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LockerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardCredentialId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockerCardCredentials", x => new { x.TenantId, x.LockerId, x.CardCredentialId });
                    table.ForeignKey(
                        name: "FK_LockerCardCredentials_CardCredentials_CardCredentialId",
                        column: x => x.CardCredentialId,
                        principalSchema: "slk",
                        principalTable: "CardCredentials",
                        principalColumn: "CardCredentialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockerCardCredentials_Lockers_LockerId",
                        column: x => x.LockerId,
                        principalSchema: "slk",
                        principalTable: "Lockers",
                        principalColumn: "LockerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Locks",
                schema: "slk",
                columns: table => new
                {
                    LockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SerialNumber = table.Column<int>(type: "int", nullable: false),
                    InstallationDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OperatingMode = table.Column<int>(type: "int", nullable: false),
                    LockerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locks", x => x.LockId);
                    table.ForeignKey(
                        name: "FK_Locks_Lockers_LockerId",
                        column: x => x.LockerId,
                        principalSchema: "slk",
                        principalTable: "Lockers",
                        principalColumn: "LockerId");
                });

            migrationBuilder.InsertData(
                schema: "slk",
                table: "StringifiedLockOperatingMode",
                columns: new[] { "Value", "Name" },
                values: new object[,]
                {
                    { 0, "Installation" },
                    { 1, "Shared" },
                    { 2, "Dedicated" },
                    { 3, "Confiscated" },
                    { 4, "Reader" }
                });

            migrationBuilder.InsertData(
                schema: "slk",
                table: "StringifiedLockerBankBehaviour",
                columns: new[] { "Value", "Name" },
                values: new object[,]
                {
                    { 0, "Unset" },
                    { 1, "Permanent" },
                    { 2, "Temporary" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardCredentials_CardHolderId",
                schema: "slk",
                table: "CardCredentials",
                column: "CardHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_CardCredentials_SerialNumber",
                schema: "slk",
                table: "CardCredentials",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardCredentials_TenantId",
                schema: "slk",
                table: "CardCredentials",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CardHolders_TenantId",
                schema: "slk",
                table: "CardHolders",
                column: "TenantId");

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
                name: "IX_Locations_TenantId",
                schema: "slk",
                table: "Locations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_TenantId_Name",
                schema: "slk",
                table: "Locations",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankAdmins_CardHolderId",
                schema: "slk",
                table: "LockerBankAdmins",
                column: "CardHolderId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankAdmins_LockerBankId",
                schema: "slk",
                table: "LockerBankAdmins",
                column: "LockerBankId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankAdmins_TenantId",
                schema: "slk",
                table: "LockerBankAdmins",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankCardCredentials_CardCredentialId",
                schema: "slk",
                table: "LockerBankCardCredentials",
                column: "CardCredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankCardCredentials_LockerBankId",
                schema: "slk",
                table: "LockerBankCardCredentials",
                column: "LockerBankId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBankCardCredentials_TenantId",
                schema: "slk",
                table: "LockerBankCardCredentials",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBanks_LocationId",
                schema: "slk",
                table: "LockerBanks",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBanks_TenantId",
                schema: "slk",
                table: "LockerBanks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerBanks_TenantId_LocationId_Name",
                schema: "slk",
                table: "LockerBanks",
                columns: new[] { "TenantId", "LocationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LockerCardCredentials_CardCredentialId",
                schema: "slk",
                table: "LockerCardCredentials",
                column: "CardCredentialId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerCardCredentials_LockerId",
                schema: "slk",
                table: "LockerCardCredentials",
                column: "LockerId");

            migrationBuilder.CreateIndex(
                name: "IX_LockerCardCredentials_TenantId",
                schema: "slk",
                table: "LockerCardCredentials",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Lockers_LockerBankId",
                schema: "slk",
                table: "Lockers",
                column: "LockerBankId");

            migrationBuilder.CreateIndex(
                name: "IX_Lockers_TenantId",
                schema: "slk",
                table: "Lockers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Lockers_TenantId_LockerBankId_Label",
                schema: "slk",
                table: "Lockers",
                columns: new[] { "TenantId", "LockerBankId", "Label" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locks_LockerId",
                schema: "slk",
                table: "Locks",
                column: "LockerId",
                unique: true,
                filter: "[LockerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Locks_SerialNumber",
                schema: "slk",
                table: "Locks",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locks_TenantId",
                schema: "slk",
                table: "Locks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_TenantId",
                schema: "slk",
                table: "Tenants",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LockerBankAdmins",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "LockerBankCardCredentials",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "LockerCardCredentials",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "Locks",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "StringifiedLockerBankBehaviour",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "StringifiedLockOperatingMode",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "Tenants",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "CardCredentials",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "Lockers",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "CardHolders",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "LockerBanks",
                schema: "slk");

            migrationBuilder.DropTable(
                name: "Locations",
                schema: "slk");
        }
    }
}
