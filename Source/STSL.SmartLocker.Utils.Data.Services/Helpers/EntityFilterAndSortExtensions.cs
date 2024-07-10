using Microsoft.VisualBasic;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Common.Exceptions;
using STSL.SmartLocker.Utils.Common.Helpers;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Domain;
using System.Linq.Expressions;

namespace STSL.SmartLocker.Utils.Data.Services.Helpers;

// NOTE: Should probably make this internal
public static class EntityFilterAndSortExtensions
{
    private static readonly string[] dateFormats = { "d/M/yy", "d/M/yyyy", "d/MM/yy", "d/MM/yyyy", "dd/M/yy", "dd/M/yyyy", "dd/MM/yy", "dd/MM/yyyy" };

    public static bool IsNullOrEmptyOrHasFilterProperty(this IFilteredRequest filter, string propertyName) => filter.FilterProperties.IsNullOrEmpty() || filter.FilterProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    public static bool HasFilterProperty(this IFilteredRequest filter, string propertyName, bool countNullOrEmptyFilterPropertiesAsTrue = false)
        => countNullOrEmptyFilterPropertiesAsTrue ? filter.IsNullOrEmptyOrHasFilterProperty(propertyName) : filter.FilterProperties?.Contains(propertyName, StringComparer.OrdinalIgnoreCase) ?? false;

    public static IOrderedQueryable<T> UseSorting<T, K>(this ISortedRequest sort, IQueryable<T> all, Expression<Func<T, K?>> keySelector)
        => sort.SortOrder == SortOrder.Ascending ? all.OrderBy(keySelector) : all.OrderByDescending(keySelector);

    public static IQueryable<Location> FilterAndSort(this IQueryable<Location> all, IFilteredRequest? filter = null, ISortedRequest? sort = null)
    {
        const string Name = "name";
        const string Description = "description";

        if (filter is not null && !string.IsNullOrWhiteSpace(filter.FilterValue))
        {
            var filterName = filter.IsNullOrEmptyOrHasFilterProperty(Name);
            var filterDescription = filter.IsNullOrEmptyOrHasFilterProperty(Description);

            all = all.Where(x =>
                (filterName && x.Name.Contains(filter.FilterValue)) ||
                (filterDescription && x.Description.Contains(filter.FilterValue))
            );
        }

        sort ??= new SortedRequest();

        all = sort.SortBy?.ToLowerInvariant() switch
        {
            Description => sort.UseSorting(all, x => x.Description).ThenBy(x => x.Name),
            _ => sort.UseSorting(all, x => x.Name)
        };

        return all;
    }

    public static IQueryable<LockerBank> FilterAndSort(this IQueryable<LockerBank> all, IFilteredRequest? filter = null, ISortedRequest? sort = null)
    {
        const string Name = "name";
        const string Description = "description";
        const string Behaviour = "behaviour";

        if (filter is not null && !string.IsNullOrWhiteSpace(filter.FilterValue))
        {
            LockerBankBehaviour behaviour = new();

            var filterName = filter.IsNullOrEmptyOrHasFilterProperty(Name);
            var filterDescription = filter.IsNullOrEmptyOrHasFilterProperty(Description);
            var filterBehaviour = filter.FilterProperties?.Length == 1 && filter.FilterProperties.Contains(Behaviour, StringComparer.OrdinalIgnoreCase) && Enum.TryParse(filter.FilterValue, ignoreCase: true, out behaviour) && Enum.IsDefined(behaviour);

            all = all.Where(x =>
                (filterName && x.Name.Contains(filter.FilterValue)) ||
                (filterDescription && x.Description.Contains(filter.FilterValue)) ||
                (filterBehaviour && x.Behaviour == behaviour)
            );
        }

        sort ??= new SortedRequest();

        all = sort.SortBy?.ToLowerInvariant() switch
        {
            Description => sort.UseSorting(all, x => x.Description).ThenBy(x => x.Name),
            Behaviour => sort.UseSorting(all, x => x.Behaviour).ThenBy(x => x.Name),
            _ => sort.UseSorting(all, x => x.Name)
        };

        return all;
    }

    internal static IQueryable<Locker> FilterAndSortLockerAndLocks(this IQueryable<Locker> all, IFilteredRequest? filter = null, ISortedRequest? sort = null)
    {
        const string Label = "label";
        const string ServiceTag = "servicetag";

        const string InstallationDate = "installationdateutc";
        const string SerialNumber = "serialnumber";

        const int NullLockSerialNumberSortValue = 0;

        if (filter is not null && !string.IsNullOrWhiteSpace(filter.FilterValue))
        {
            DateTimeOffset installDate = new();
            int serialNumber = 0;

            var filterInstallationDate = filter.IsNullOrEmptyOrHasFilterProperty(InstallationDate) && DateTimeOffset.TryParseExact(filter.FilterValue, dateFormats, null, System.Globalization.DateTimeStyles.AssumeUniversal, out installDate);
            var filterSerialNumber = filter.IsNullOrEmptyOrHasFilterProperty(SerialNumber) && int.TryParse(filter.FilterValue, out serialNumber);
            var filterLabel = filter.IsNullOrEmptyOrHasFilterProperty(Label);
            var filterServiceTag = filter.IsNullOrEmptyOrHasFilterProperty(ServiceTag);

            all = all.Where(x =>
                (filterInstallationDate && x.Lock != null && installDate == x.Lock.InstallationDateUtc.Date) ||
                (filterSerialNumber && x.Lock != null && x.Lock.SerialNumber == serialNumber) ||
                (filterLabel && x.Label.Contains(filter.FilterValue)) ||
                (filterServiceTag && x.ServiceTag != null && x.ServiceTag.Contains(filter.FilterValue))
            );
        }

        sort ??= new SortedRequest();

        all = sort.SortBy?.ToLowerInvariant() switch
        {
            SerialNumber => sort.UseSorting(all, x => x.Lock != null ? x.Lock.SerialNumber : NullLockSerialNumberSortValue),
            ServiceTag => sort.UseSorting(all, x => x.ServiceTag),
            _ => sort.UseSorting(all, x => x.Label)
        };

        return all;
    }

    public static IQueryable<CardHolder> FilterAndSort(this IQueryable<CardHolder> all, IFilteredRequest? filter = null, ISortedRequest? sort = null)
    {
        const string FirstName = "firstname";
        const string LastName = "lastname";
        const string Email = "email";
        const string UniqueIdentifier = "uniqueidentifier";
        
        if (filter is not null && !string.IsNullOrWhiteSpace(filter.FilterValue))
        {
            var filterFirstName = filter.IsNullOrEmptyOrHasFilterProperty(FirstName);
            var filterLastName = filter.IsNullOrEmptyOrHasFilterProperty(LastName);
            var filterEmail = filter.IsNullOrEmptyOrHasFilterProperty(Email);
            var filterUniqueIdentifier = filter.IsNullOrEmptyOrHasFilterProperty(UniqueIdentifier);

            if (filter.StartsWith)
            {
                all = all.Where(x =>
                    (filterFirstName && x.FirstName.StartsWith(filter.FilterValue)) ||
                    (filterLastName && x.LastName.StartsWith(filter.FilterValue)) ||
                    (filterEmail && x.Email != null && x.Email.StartsWith(filter.FilterValue)) ||
                    (filterUniqueIdentifier && x.UniqueIdentifier != null && x.UniqueIdentifier.StartsWith(filter.FilterValue))
                );
            }

            if (filterFirstName && filterLastName)
            {
                all = all.Where(x =>
                    (x.FirstName + " " + x.LastName).Contains(filter.FilterValue) ||
                    (filterEmail && x.Email != null && x.Email.Contains(filter.FilterValue)) ||
                    (filterUniqueIdentifier && x.UniqueIdentifier != null && x.UniqueIdentifier.Contains(filter.FilterValue))
                );
            }
            else
            {
                all = all.Where(x =>
                    (filterFirstName && x.FirstName.Contains(filter.FilterValue)) ||
                    (filterLastName && x.LastName.Contains(filter.FilterValue)) ||
                    (filterEmail && x.Email != null && x.Email.Contains(filter.FilterValue)) ||
                    (filterUniqueIdentifier && x.UniqueIdentifier != null && x.UniqueIdentifier.Contains(filter.FilterValue))
                );
            }

        }

        sort ??= new SortedRequest();

        all = sort.SortBy?.ToLowerInvariant() switch
        {
            FirstName => sort.UseSorting(all, x => x.FirstName).ThenBy(x => x.LastName),
            Email => sort.UseSorting(all, x => x.Email).ThenBy(x => x.LastName),
            UniqueIdentifier => sort.UseSorting(all, x => x.UniqueIdentifier).ThenBy(x => x.LastName),
            _ => sort.UseSorting(all, x => x.LastName).ThenBy(x => x.FirstName),
        };

        return all;
    }
}
