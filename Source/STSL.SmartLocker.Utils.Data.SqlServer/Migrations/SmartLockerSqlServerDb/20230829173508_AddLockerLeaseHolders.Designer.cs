﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using STSL.SmartLocker.Utils.Data.SqlServer.Contexts;

#nullable disable

namespace STSL.SmartLocker.Utils.Data.SqlServer.Migrations.SmartLockerSqlServerDb
{
    [DbContext(typeof(SmartLockerSqlServerDbContext))]
    [Migration("20230829173508_AddLockerLeaseHolders")]
    partial class AddLockerLeaseHolders
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("slk")
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results.ListAuditRecordsForLocker_Result", b =>
                {
                    b.Property<long>("RowNum")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("RowNum"));

                    b.Property<string>("AuditCategory")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AuditDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("AuditTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("AuditType")
                        .HasColumnType("int");

                    b.Property<int>("LockSerialNumber")
                        .HasColumnType("int");

                    b.Property<string>("Object")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ObjectSN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Subject")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SubjectSN")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RowNum");

                    b.ToTable("ListAuditRecordsForLocker_Results", "slk", t =>
                        {
                            t.ExcludeFromMigrations();
                        });
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Data.SqlServer.Views.LockersWithStatus_View", b =>
                {
                    b.Property<Guid>("LockerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AssignedTo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("AssignedToCardHolderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AssignedToUniqueIdentifier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal?>("Battery")
                        .HasColumnType("decimal(4,3)");

                    b.Property<int?>("BoundaryAddress")
                        .HasColumnType("int");

                    b.Property<byte?>("LastAudit")
                        .HasColumnType("tinyint");

                    b.Property<string>("LastAuditCategory")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastAuditDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastAuditObject")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("LastAuditObjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LastAuditObjectSN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastAuditObjecttUniqueIdentifier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastAuditSubject")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("LastAuditSubjectId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LastAuditSubjectSN")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastAuditSubjectUniqueIdentifier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("LastAuditTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("LastCommunication")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LockFirmwareVersion")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("LockId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("LockOperatingMode")
                        .HasColumnType("int");

                    b.Property<int>("LockSerialNumber")
                        .HasColumnType("int");

                    b.Property<string>("Locker")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("LockerBankId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("LockerId");

                    b.ToTable((string)null);

                    b.ToView("LockersWithStatus", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.CardCredential", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("CardCredentialId");

                    b.Property<Guid?>("CardHolderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CardLabel")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int>("CardType")
                        .HasColumnType("int");

                    b.Property<string>("HidNumber")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<string>("SerialNumber")
                        .HasMaxLength(16)
                        .IsUnicode(false)
                        .HasColumnType("char(16)")
                        .IsFixedLength();

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CardHolderId");

                    b.HasIndex("HidNumber")
                        .IsUnique();

                    b.HasIndex("SerialNumber")
                        .IsUnique()
                        .HasFilter("[SerialNumber] IS NOT NULL");

                    b.HasIndex("TenantId");

                    b.ToTable("CardCredentials", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.CardHolder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("CardHolderId");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("UniqueIdentifier")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.HasIndex("TenantId", "Email")
                        .IsUnique()
                        .HasFilter("[Email] IS NOT NULL");

                    b.HasIndex("TenantId", "UniqueIdentifier")
                        .IsUnique()
                        .HasFilter("[UniqueIdentifier] IS NOT NULL");

                    b.ToTable("CardHolders", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Location", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("LocationId");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.HasIndex("TenantId", "Name")
                        .IsUnique();

                    b.ToTable("Locations", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Lock", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("LockId");

                    b.Property<string>("FirmwareVersion")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<DateTimeOffset>("InstallationDateUtc")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("LockerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("OperatingMode")
                        .HasColumnType("int");

                    b.Property<int>("SerialNumber")
                        .HasColumnType("int");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("LockerId")
                        .IsUnique()
                        .HasFilter("[LockerId] IS NOT NULL");

                    b.HasIndex("SerialNumber")
                        .IsUnique();

                    b.HasIndex("TenantId");

                    b.ToTable("Locks", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockConfigEventAudit", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("LockConfigEventAuditId");

                    b.Property<DateTimeOffset>("CreatedAtUTC")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("EntityId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("EventType")
                        .HasColumnType("int");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("UpdatedByUserEmail")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("LockConfigEventAudits", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Locker", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("LockerId");

                    b.Property<DateTimeOffset?>("AbsoluteLeaseExpiry")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("CardHolderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CurrentLeaseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("LockerBankId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ServiceTag")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CardHolderId");

                    b.HasIndex("CurrentLeaseId")
                        .IsUnique()
                        .HasFilter("[CurrentLeaseId] IS NOT NULL");

                    b.HasIndex("LockerBankId");

                    b.HasIndex("TenantId");

                    b.HasIndex("TenantId", "ServiceTag")
                        .IsUnique()
                        .HasFilter("[ServiceTag] IS NOT NULL");

                    b.HasIndex("TenantId", "LockerBankId", "Label")
                        .IsUnique();

                    b.ToTable("Lockers", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerBank", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("LockerBankId");

                    b.Property<int>("Behaviour")
                        .HasColumnType("int");

                    b.Property<TimeSpan?>("DefaultLeaseDuration")
                        .HasColumnType("time");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("LocationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.HasIndex("TenantId");

                    b.HasIndex("TenantId", "LocationId", "Name")
                        .IsUnique();

                    b.ToTable("LockerBanks", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerBankAdmin", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("LockerBankId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CardHolderId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TenantId", "LockerBankId", "CardHolderId");

                    b.HasIndex("CardHolderId");

                    b.HasIndex("LockerBankId");

                    b.HasIndex("TenantId");

                    b.ToTable("LockerBankAdmins", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerBankCardCredential", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("LockerBankId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CardCredentialId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TenantId", "LockerBankId", "CardCredentialId");

                    b.HasIndex("CardCredentialId");

                    b.HasIndex("LockerBankId");

                    b.HasIndex("TenantId");

                    b.ToTable("LockerBankCardCredentials", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerCardCredential", b =>
                {
                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("LockerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CardCredentialId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("TenantId", "LockerId", "CardCredentialId");

                    b.HasIndex("CardCredentialId");

                    b.HasIndex("LockerId");

                    b.HasIndex("TenantId");

                    b.ToTable("LockerCardCredentials", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerLease", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("LockerLeaseId");

                    b.Property<Guid?>("CardCredentialId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CardHolderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("EndedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("LockId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("LockerBankBehaviour")
                        .HasColumnType("int");

                    b.Property<Guid?>("LockerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("StartedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CardCredentialId");

                    b.HasIndex("CardHolderId");

                    b.HasIndex("LockId");

                    b.HasIndex("LockerId");

                    b.HasIndex("TenantId");

                    b.ToTable("LockerLeases", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.StringifiedLockOperatingMode", b =>
                {
                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Value");

                    b.ToTable("StringifiedLockOperatingMode", "slk");

                    b.HasData(
                        new
                        {
                            Value = 0,
                            Name = "Installation"
                        },
                        new
                        {
                            Value = 1,
                            Name = "Shared"
                        },
                        new
                        {
                            Value = 2,
                            Name = "Dedicated"
                        },
                        new
                        {
                            Value = 3,
                            Name = "Confiscated"
                        },
                        new
                        {
                            Value = 4,
                            Name = "Reader"
                        });
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.StringifiedLockerBankBehaviour", b =>
                {
                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Value");

                    b.ToTable("StringifiedLockerBankBehaviour", "slk");

                    b.HasData(
                        new
                        {
                            Value = 0,
                            Name = "Unset"
                        },
                        new
                        {
                            Value = 1,
                            Name = "Permanent"
                        },
                        new
                        {
                            Value = 2,
                            Name = "Temporary"
                        });
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Tenant", b =>
                {
                    b.Property<Guid>("TenantId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("AllowLockUpdates")
                        .HasColumnType("bit");

                    b.Property<string>("CardHolderAlias")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<byte[]>("Logo")
                        .HasMaxLength(2097152)
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("LogoMimeType")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("TenantId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("TenantId");

                    b.ToTable("Tenants", "slk");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.CardCredential", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.CardHolder", "CardHolder")
                        .WithMany("CardCredentials")
                        .HasForeignKey("CardHolderId");

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CardHolder");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.CardHolder", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Location", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Lock", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.Locker", "Locker")
                        .WithOne("Lock")
                        .HasForeignKey("STSL.SmartLocker.Utils.Domain.Lock", "LockerId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Locker");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockConfigEventAudit", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Locker", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.CardHolder", null)
                        .WithMany("CurrentlyLeasedLockers")
                        .HasForeignKey("CardHolderId");

                    b.HasOne("STSL.SmartLocker.Utils.Domain.LockerLease", "CurrentLease")
                        .WithOne()
                        .HasForeignKey("STSL.SmartLocker.Utils.Domain.Locker", "CurrentLeaseId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("STSL.SmartLocker.Utils.Domain.LockerBank", "LockerBank")
                        .WithMany("Lockers")
                        .HasForeignKey("LockerBankId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CurrentLease");

                    b.Navigation("LockerBank");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerBank", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.Location", "Location")
                        .WithMany("LockerBanks")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Location");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerBankAdmin", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.CardHolder", "CardHolder")
                        .WithMany()
                        .HasForeignKey("CardHolderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("STSL.SmartLocker.Utils.Domain.LockerBank", "LockerBank")
                        .WithMany("Admins")
                        .HasForeignKey("LockerBankId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CardHolder");

                    b.Navigation("LockerBank");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerBankCardCredential", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.CardCredential", "CardCredential")
                        .WithMany("LockerBankCardCredentials")
                        .HasForeignKey("CardCredentialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("STSL.SmartLocker.Utils.Domain.LockerBank", "LockerBank")
                        .WithMany("CardCredentials")
                        .HasForeignKey("LockerBankId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CardCredential");

                    b.Navigation("LockerBank");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerCardCredential", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.CardCredential", "CardCredential")
                        .WithMany("LockCardCredentials")
                        .HasForeignKey("CardCredentialId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Locker", "Locker")
                        .WithMany("CardCredentials")
                        .HasForeignKey("LockerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CardCredential");

                    b.Navigation("Locker");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerLease", b =>
                {
                    b.HasOne("STSL.SmartLocker.Utils.Domain.CardCredential", "CardCredential")
                        .WithMany()
                        .HasForeignKey("CardCredentialId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("STSL.SmartLocker.Utils.Domain.CardHolder", "CardHolder")
                        .WithMany("LockerLeaseHistory")
                        .HasForeignKey("CardHolderId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Lock", "Lock")
                        .WithMany()
                        .HasForeignKey("LockId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Locker", "Locker")
                        .WithMany("LeaseHistory")
                        .HasForeignKey("LockerId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("STSL.SmartLocker.Utils.Domain.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CardCredential");

                    b.Navigation("CardHolder");

                    b.Navigation("Lock");

                    b.Navigation("Locker");

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.CardCredential", b =>
                {
                    b.Navigation("LockCardCredentials");

                    b.Navigation("LockerBankCardCredentials");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.CardHolder", b =>
                {
                    b.Navigation("CardCredentials");

                    b.Navigation("CurrentlyLeasedLockers");

                    b.Navigation("LockerLeaseHistory");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Location", b =>
                {
                    b.Navigation("LockerBanks");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.Locker", b =>
                {
                    b.Navigation("CardCredentials");

                    b.Navigation("LeaseHistory");

                    b.Navigation("Lock");
                });

            modelBuilder.Entity("STSL.SmartLocker.Utils.Domain.LockerBank", b =>
                {
                    b.Navigation("Admins");

                    b.Navigation("CardCredentials");

                    b.Navigation("Lockers");
                });
#pragma warning restore 612, 618
        }
    }
}
