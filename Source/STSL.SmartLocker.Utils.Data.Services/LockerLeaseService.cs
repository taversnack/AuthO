using Microsoft.EntityFrameworkCore;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Contexts;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Mappings;
using STSL.SmartLocker.Utils.Domain;
using STSL.SmartLocker.Utils.DTO;
using System.ComponentModel.DataAnnotations;

namespace STSL.SmartLocker.Utils.Data.Services;

public sealed class LockerLeaseService : ILockerLeaseService
{
    private readonly IRepository<LockerLease> _repository;
    private readonly SmartLockerDbContext _dbContext;

    public LockerLeaseService(IRepository<LockerLease> repository, SmartLockerDbContext dbContext) 
        => (_repository, _dbContext) = (repository, dbContext);

    public async Task<LockerLeaseDTO?> GetLockerLeaseAsync(Guid tenantId, Guid lockerLeaseId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, lockerLeaseId, cancellationToken);

        return entity is null ? null : LockerLeaseMapper.ToDTO(entity);
    }

    public async Task<IReadOnlyList<LockerLeaseDTO>> GetLockerLeaseHistoryAsync(Guid tenantId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortLockerLeasesAsync(_repository.QueryingAll(tenantId)
                .Include(x => x.CardCredential)
                .Include(x => x.CardHolder)
                .Include(x => x.Locker)
                .Include(x => x.Lock), 
            filter, sort, cancellationToken);


    public async Task<IReadOnlyList<LockerLeaseDTO>> GetLockerLeaseHistoryByCardHolderAsync(Guid tenantId, Guid cardHolderId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortLockerLeasesAsync(_repository.QueryingAll(tenantId)
                .Include(x => x.CardCredential)
                .Include(x => x.Locker)
                .Include(x => x.Lock)
                .Where(x => x.CardHolderId == cardHolderId),
            filter, sort, cancellationToken);

    public async Task<IReadOnlyList<LockerLeaseDTO>> GetLockerLeaseHistoryByLockerAsync(Guid tenantId, Guid lockerId, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
        => await FilterAndSortLockerLeasesAsync(_repository.QueryingAll(tenantId)
                .Include(x => x.CardCredential)
                .Include(x => x.CardHolder)
                .Include(x => x.Lock)
                .Where(x => x.LockerId == lockerId), 
            filter, sort, cancellationToken);

    public async Task<LockerLeaseDTO?> StartPermanentLockerLeaseAsync(Guid tenantId, CreatePermanentLockerLeaseDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = CreatePermanentLockerLeaseMapper.ToEntity(dto);

        //_repository.ValidateOne(entity, _validator);

        var newEntity = await _repository.CreateOneAsync(tenantId, entity, cancellationToken);

        return newEntity is null ? null : LockerLeaseMapper.ToDTO(newEntity);
    }
    public async Task<LockerLeaseDTO?> EndPermanentLockerLeaseAsync(Guid tenantId, Guid lockerLeaseId, DateTimeOffset endedAt, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetOneAsync(tenantId, lockerLeaseId, cancellationToken);

        if(entity is null)
        {
            return null;
        }

        entity.EndedAt = endedAt;

        await _repository.UpdateOneAsync(tenantId, lockerLeaseId, entity, cancellationToken);

        return LockerLeaseMapper.ToDTO(entity);
    }

    private static async Task<IReadOnlyList<LockerLeaseDTO>> FilterAndSortLockerLeasesAsync(IQueryable<LockerLease> all, IFilteredRequest? filter = null, ISortedRequest? sort = null, CancellationToken cancellationToken = default)
    {
        const string StartedAt = "startedat";
        //const string EndedAt = "endedat";
        const string CardHolderName = "cardholdername";
        const string CardHolderUniqueIdentifier = "cardholderuniqueidentifier";
        const string CardHolderEmail = "cardholderemail";
        const string LockerServiceTag = "lockerservicetag";
        const string LockerLabel = "lockerlabel";
        const string LockerBankBehaviour = "lockerbankbehaviour";

        if (filter is not null && !filter.FilterValue.IsNullOrWhiteSpace())
        {
            LockerBankBehaviour lockerBankBehaviour = new();

            var filterCardHolderName = filter.IsNullOrEmptyOrHasFilterProperty(CardHolderName);
            var filterCardHolderUniqueIdentifier = filter.IsNullOrEmptyOrHasFilterProperty(CardHolderUniqueIdentifier);
            var filterCardHolderEmail = filter.IsNullOrEmptyOrHasFilterProperty(CardHolderEmail);
            var filterLockerServiceTag = filter.IsNullOrEmptyOrHasFilterProperty(LockerServiceTag);
            var filterLockerLabel = filter.IsNullOrEmptyOrHasFilterProperty(LockerLabel);

            var filterLockerBankBehaviour = filter.FilterProperties?.Length == 1 
                && filter.FilterProperties.Contains(LockerBankBehaviour, StringComparer.OrdinalIgnoreCase) 
                && Enum.TryParse(filter.FilterValue, ignoreCase: true, out lockerBankBehaviour)
                && Enum.IsDefined(lockerBankBehaviour);

            // NOTE: Could also filter startedAt / endedAt using a single date or parsing a date range eg filterValue = "date1 date2"
            // split string then tryparse each assuming filterProperty is StartedAt / EndedAt

            all = all.Where(x =>
                (filterCardHolderName && x.CardHolder != null && (x.CardHolder.FirstName + " " + x.CardHolder.LastName).Contains(filter.FilterValue)) ||
                (filterCardHolderUniqueIdentifier && x.CardHolder != null && x.CardHolder.UniqueIdentifier != null && x.CardHolder.UniqueIdentifier.Contains(filter.FilterValue)) ||
                (filterCardHolderEmail && x.CardHolder != null && x.CardHolder.Email != null && x.CardHolder.Email.Contains(filter.FilterValue)) ||
                (filterLockerServiceTag && x.Locker != null && x.Locker.ServiceTag != null && x.Locker.ServiceTag.Contains(filter.FilterValue)) ||
                (filterLockerLabel && x.Locker != null && x.Locker.Label.Contains(filter.FilterValue)) ||
                (filterLockerBankBehaviour && x.LockerBankBehaviour == lockerBankBehaviour)
            );
        }

        sort ??= new SortedRequest();

        all = sort.SortBy?.ToLowerInvariant() switch
        {
            CardHolderName => sort.UseSorting(all, x => x.CardHolder == null ? null : x.CardHolder.LastName).ThenBy(x => x.CardHolder == null ? null : x.CardHolder.FirstName),
            StartedAt => sort.UseSorting(all, x => x.StartedAt).ThenBy(x => x.EndedAt),
            _ => sort.UseSorting(all, x => x.EndedAt).ThenBy(x => x.StartedAt)
        };

        return await all.Select(x => LockerLeaseMapper.ToDTO(x)).ToListAsync(cancellationToken);
    }
}
