using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Configuration;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services;

public sealed class LockService : ILockService
{
    private readonly IRepository<Lock> _repository;
    private readonly IValidator<Lock> _validator;
    private readonly GlobalServiceOptions _options;

    public LockService(IRepository<Lock> repository, IValidator<Lock> validator, IOptions<GlobalServiceOptions> options)
        => (_repository, _validator, _options) = (repository, validator, options.Value);

    public async Task<LockDTO?> CreateLockAsync(Guid tenantId, CreateLockDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreateLockMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        var newEntity = await _repository.CreateOneAsync(tenantId, entity, cancellationToken);

        return newEntity is null ? null : LockMapper.ToDTO(newEntity);
    }

    public async Task<bool> CreateManyLocksAsync(Guid tenantId, IEnumerable<CreateLockDTO> dtoList, CancellationToken cancellationToken = default)
    {
        var dtoCount = dtoList.TryGetNonEnumeratedCount(out var count) ? count : dtoList.Count();

        return (await CreateAndUseManyLocksAsync(tenantId, dtoList, cancellationToken)).Count == dtoCount;
    }

    public async Task<IReadOnlyList<LockDTO>> CreateAndUseManyLocksAsync(Guid tenantId, IEnumerable<CreateLockDTO> dtoList, CancellationToken cancellationToken = default)
    {
        if (!dtoList.Any())
        {
            return _options.EmptyBulkOperationIsError ? throw new BadRequestException("No data was passed") : new List<LockDTO>();
        }

        var entities = dtoList.Select(CreateLockMapper.ToEntity).ToList();

        _repository.ValidateMany(entities, _validator, _options.ThrowOnFirstValidationErrorForBulkOperations);

        await _repository.CreateManyAsync(tenantId, entities, cancellationToken);

        return entities.ConvertAll(LockMapper.ToDTO);
    }

    public async Task<LockDTO?> GetLockAsync(Guid tenantId, Guid lockId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, lockId, cancellationToken);

        return entity is null ? null : LockMapper.ToDTO(entity);
    }

    public async Task<LockDTO?> GetLockByLockerIdAsync(Guid tenantId, Guid lockerId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryingAll(tenantId).Where(x => x.LockerId == lockerId).FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : LockMapper.ToDTO(entity);
    }

    public async Task<LockDTO?> GetLockBySerialNumberAsync(Guid tenantId, LockSerial lockSerial, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.QueryingAll(tenantId).Where(x => x.SerialNumber == lockSerial).FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : LockMapper.ToDTO(entity);
    }

    public async Task<IReadOnlyList<LockDTO>> GetManyLocksAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortLocksAsync(_repository.QueryingAll(tenantId), filter, sort, cancellationToken);

    public async Task UpdateLockAsync(Guid tenantId, Guid lockId, UpdateLockDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = UpdateLockMapper.ToEntity(dto);

        _repository.ValidateOne(entity, _validator);

        if (dto.OverrideExistingLockerLockPair && dto.LockerId is not null)
        {
            var lockersCurrentLock = await _repository.QueryingAll(tenantId).FirstOrDefaultAsync(x => x.LockerId == dto.LockerId, cancellationToken);

            if (lockersCurrentLock is not null)
            {
                lockersCurrentLock.LockerId = null;
            }

            await _repository.SaveChangesAsync(cancellationToken);
        }

        await _repository.UpdateOneAsync(tenantId, lockId, entity, cancellationToken);
    }

    public async Task DeleteLockAsync(Guid tenantId, Guid lockId, CancellationToken cancellationToken = default)
        => await _repository.DeleteOneAsync(tenantId, lockId, _options.ThrowNotFoundWhenDeletingNonExistantEntity, cancellationToken);


    private static readonly string[] dateFormats = { "d/M/yy", "d/M/yyyy", "d/MM/yy", "d/MM/yyyy", "dd/M/yy", "dd/M/yyyy", "dd/MM/yy", "dd/MM/yyyy" };

    private static async Task<IReadOnlyList<LockDTO>> FilterAndSortLocksAsync(IQueryable<Lock> all, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    {
        const string SerialNumber = "serialnumber";
        const string InstallationDate = "installationdateutc";
        const string FirmwareVersion = "firmwareversion";
        const string OperatingMode = "operatingmode";

        if (filter is not null && !string.IsNullOrWhiteSpace(filter.FilterValue))
        {
            DateTimeOffset installDate = new();
            LockOperatingMode operatingMode = new();
            int serialNumber = 0;

            var filterSerialNumber = (filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(SerialNumber, StringComparer.OrdinalIgnoreCase)) && int.TryParse(filter.FilterValue, out serialNumber);
            var filterInstallationDate = (filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(InstallationDate, StringComparer.OrdinalIgnoreCase)) && DateTimeOffset.TryParseExact(filter.FilterValue, dateFormats, null, System.Globalization.DateTimeStyles.AssumeUniversal, out installDate);
            var filterFirmwareVersion = filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(FirmwareVersion, StringComparer.OrdinalIgnoreCase);
            var filterOperatingMode = (filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(OperatingMode, StringComparer.OrdinalIgnoreCase)) && Enum.TryParse(filter.FilterValue, ignoreCase: true, out operatingMode) && Enum.IsDefined(operatingMode);


            all = all.Where(x =>
                (filterSerialNumber && x.SerialNumber == serialNumber) ||
                (filterFirmwareVersion && x.FirmwareVersion.Contains(filter.FilterValue)) ||
                (filterInstallationDate && installDate == x.InstallationDateUtc.Date) ||
                (filterOperatingMode && x.OperatingMode == operatingMode)
            );
        }

        if (sort is not null && !string.IsNullOrWhiteSpace(sort.SortBy))
        {
            all = sort.SortBy.ToLowerInvariant() switch
            {
                SerialNumber => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.SerialNumber) : all.OrderByDescending(x => x.SerialNumber),
                InstallationDate => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.InstallationDateUtc) : all.OrderByDescending(x => x.InstallationDateUtc),
                FirmwareVersion => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.FirmwareVersion) : all.OrderByDescending(x => x.FirmwareVersion),
                OperatingMode => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(x => x.OperatingMode) : all.OrderByDescending(x => x.OperatingMode),
                _ => throw new BadRequestException($"Cannot sort on property {sort.SortBy}, no such property exists")
            };
        }

        return await all.Select(x => LockMapper.ToDTO(x)).ToListAsync(cancellationToken);
    }
}
