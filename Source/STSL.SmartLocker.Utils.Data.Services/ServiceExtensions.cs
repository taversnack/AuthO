using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using STSL.SmartLocker.Utils.BlubugConfigProducer;
using STSL.SmartLocker.Utils.Data.Services.Configuration;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Validators;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.Kiosk.Printer.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Kiosk;
using STSL.SmartLocker.Utils.AzureServiceBus;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Kiosk;
using STSL.SmartLocker.Utils.Kiosk.Models.Printer;

namespace STSL.SmartLocker.Utils.Data.Services;

public static class ServiceExtensions
{
    private const string DefaultGlobalServiceOptionsSection = nameof(GlobalServiceOptions);

    private const string AzureReferenceImageConnectionString = "AzureImageConnectionString";

    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(IRepository<>), typeof(Repository<>));

        services.TryAddTransient(typeof(IReferenceImageRepository<,>), typeof(ReferenceImageRepository<,>));

        services.TryAddValidators();

        services.TryAddDomainServices();

        services.AddBlubugServices();
    }

    public static void AddServiceOptions(this IServiceCollection services, IConfiguration configuration, string globalServiceOptionsSection = DefaultGlobalServiceOptionsSection)
    {
        services.Configure<GlobalServiceOptions>(configuration.GetSection(globalServiceOptionsSection));

        services.ConfigureBlubugServices(configuration);

        services.ConfigureAzureServiceBus(configuration);

        services.AddScoped<IKioskService, KioskService>();
        // Configure the Azure Storage Service
        services.TryAddAzureStorageBlobServices(configuration);

        var section = configuration.GetSection(EmailSettings.Name);
        services.Configure<EmailSettings>(section);

    }
    private static void TryAddValidators(this IServiceCollection services)
    {
        services.TryAddSingleton<IValidator<Location>, LocationValidator>();
        services.TryAddSingleton<IValidator<LockerBank>, LockerBankValidator>();
        services.TryAddSingleton<IValidator<Locker>, LockerValidator>();
        services.TryAddSingleton<IValidator<Lock>, LockValidator>();
        services.TryAddSingleton<IValidator<CardHolder>, CardHolderValidator>();
        services.TryAddSingleton<IValidator<CardCredential>, CardCredentialValidator>();
        services.TryAddSingleton<IValidator<LockerBankAdmin>, LockerBankAdminValidator>();
    }

    /*
    private static void TryAddValidators(this IServiceCollection services)
    {
        // TODO: Benchmark injecting transient / singleton with reference types as above
        // vs validators as readonly structs with transient / singleton factory method like below
        services.TryAddTransient<IValidator<Location>>(_ => new LocationValidator());
        services.TryAddTransient<IValidator<LockerBank>>(_ => new LockerBankValidator());
        services.TryAddTransient<IValidator<Locker>>(_ => new LockerValidator());
        services.TryAddTransient<IValidator<Lock>>(_ => new LockValidator());
        // ...
    }
    */

    private static void TryAddDomainServices(this IServiceCollection services)
    {
        services.TryAddTransient<ITenantService, TenantService>();
        services.TryAddTransient<ILocationService, LocationService>();
        services.TryAddTransient<ILockerBankService, LockerBankService>();
        services.TryAddTransient<ILockerService, LockerService>();
        services.TryAddTransient<ILockService, LockService>();
        services.TryAddTransient<ICardHolderService, CardHolderService>();
        services.TryAddTransient<ICardCredentialService, CardCredentialService>();
        services.TryAddTransient<ILockerBankAdminService, LockerBankAdminService>();
        services.TryAddTransient<ILockConfigService, LockConfigService>();
        services.TryAddTransient<ILockConfigAuditService, LockConfigAuditService>();
        services.TryAddTransient<IBulkOperationService, BulkOperationService>();
        services.TryAddTransient<ILockerLeaseService, LockerLeaseService>();
        services.TryAddTransient(typeof(IReferenceImageService<,>), typeof(ReferenceImageService<,>));
        services.TryAddTransient<IEmailService, EmailService>();
    }

    private static void TryAddAzureStorageBlobServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(x => new BlobServiceClient(configuration.GetConnectionString(AzureReferenceImageConnectionString)));
        services.TryAddTransient<IAzureBlobService, AzureBlobService>();
    }
}
