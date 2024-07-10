using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using STSL.SmartLocker.Utils.Common.Data;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

namespace STSL.SmartLocker.Utils.Data.Services.Helpers;

// Convenience class for simple IPagingResponse implementation
public sealed class PagingResponse<T> : IPagingResponse<T>
{
    public int PageIndex { get; init; }
    public int RecordsPerPage { get; init; }
    public int RecordCount { get; init; }
    public int TotalRecords { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyCollection<T> Results { get; init; } = Array.Empty<T>();
}

public sealed class PagingRequest : IPagingRequest
{
    private static readonly int _defaultRecordsPerPage = 50;
    public int PageIndex { get; init; } = 0;
    public int RecordsPerPage { get; init; } = _defaultRecordsPerPage;
}

public sealed class FilteredRequest : IFilteredRequest
{
    private readonly string _filterValue = string.Empty;
    public FilteredRequest() => (FilterProperties, FilterValue) = (null, string.Empty);

    public FilteredRequest(string filterProperties, string filterValue, bool startsWith)
        => (FilterProperties, FilterValue, StartsWith) = (filterProperties.Split(','), filterValue, startsWith);

    public string[]? FilterProperties { get; init; } = Array.Empty<string>();
    public string FilterValue { get => _filterValue; init => _filterValue = value.Replace("%20", " ").Replace("%2F", "/").Trim(); }
    public bool StartsWith { get; init; }
}

public sealed class SortedRequest : ISortedRequest
{
    public string? SortBy { get; init; }
    public SortOrder SortOrder { get; init; }
}

public static class RequestExtensions
{
    #region IPagingRequest

    public static PagingResponse<T> ToResponse<T>(this IPagingRequest request, IEnumerable<T> source)
    {
        var results = source
            .Skip(request.RecordsPerPage * request.PageIndex)
            .Take(request.RecordsPerPage)
            .ToList();

        int resultsCount = results.Count;
        int totalRecords = source.TryGetNonEnumeratedCount(out int total) ? total : source.Count();

        return new PagingResponse<T>
        {
            PageIndex = request.PageIndex,
            RecordsPerPage = request.RecordsPerPage,
            RecordCount = resultsCount,
            TotalRecords = totalRecords,
            TotalPages = (totalRecords + request.RecordsPerPage - 1) / request.RecordsPerPage,
            Results = results
        };
    }

    public static PagingResponse<T> ToResponse<T, S>(this IPagingRequest request, IEnumerable<S> source, Func<S, int, T> selector)
    {
        var results = source
            .Skip(request.RecordsPerPage * request.PageIndex)
            .Take(request.RecordsPerPage)
            .Select(selector)
            .ToList();

        int resultsCount = results.Count;
        int totalRecords = source.TryGetNonEnumeratedCount(out int total) ? total : source.Count();

        return new PagingResponse<T>
        {
            PageIndex = request.PageIndex,
            RecordsPerPage = request.RecordsPerPage,
            RecordCount = resultsCount,
            TotalRecords = totalRecords,
            TotalPages = (totalRecords + request.RecordsPerPage - 1) / request.RecordsPerPage,
            Results = results
        };
    }

    public static U ToResponse<T, U>(this IPagingRequest request, IEnumerable<T> source) where U : IPagingResponse<T>, new()
    {
        var results = source
            .Skip(request.RecordsPerPage * request.PageIndex)
            .Take(request.RecordsPerPage)
            .ToList();

        int resultsCount = results.Count;
        int totalRecords = source.TryGetNonEnumeratedCount(out int total) ? total : source.Count();

        return new U
        {
            PageIndex = request.PageIndex,
            RecordsPerPage = request.RecordsPerPage,
            RecordCount = resultsCount,
            TotalRecords = totalRecords,
            TotalPages = (totalRecords + request.RecordsPerPage - 1) / request.RecordsPerPage,
            Results = results
        };
    }

    public static U ToResponse<T, S, U>(this IPagingRequest request, IEnumerable<S> source, Func<S, int, T> selector) where U : IPagingResponse<T>, new()
    {
        var results = source
            .Skip(request.RecordsPerPage * request.PageIndex)
            .Take(request.RecordsPerPage)
            .Select(selector)
            .ToList();

        int resultsCount = results.Count;
        int totalRecords = source.TryGetNonEnumeratedCount(out int total) ? total : source.Count();

        return new U
        {
            PageIndex = request.PageIndex,
            RecordsPerPage = request.RecordsPerPage,
            RecordCount = resultsCount,
            TotalRecords = totalRecords,
            TotalPages = (totalRecords + request.RecordsPerPage - 1) / request.RecordsPerPage,
            Results = results
        };
    }

    public async static Task<PagingResponse<T>> ToResponseAsync<T>(this IPagingRequest request, IQueryable<T> source, CancellationToken cancellationToken = default)
    {
        // NOTE: This may cause problems with EF when used with includes due to the way EF generates the count query
        // and does not use joins inside the count (problematic for getting true count with inner joins).
        // To avoid this you must load all data from the database then count it afterwards
        // (comment the totalRecords below and uncomment the one further down).
        // Last I checked this issues was being considered by the EF team.

        var totalRecords = await source.CountAsync();

        var results = await source
            .Skip(request.RecordsPerPage * request.PageIndex)
            .Take(request.RecordsPerPage)
            .ToListAsync(cancellationToken);

        //int totalRecords = source.TryGetNonEnumeratedCount(out int total) ? total : source.Count();
        
        int resultsCount = results.Count;

        return new PagingResponse<T>
        {
            PageIndex = request.PageIndex,
            RecordsPerPage = request.RecordsPerPage,
            RecordCount = resultsCount,
            TotalRecords = totalRecords,
            TotalPages = (totalRecords + request.RecordsPerPage - 1) / request.RecordsPerPage,
            Results = results
        };
    }

    public async static Task<U> ToResponseAsync<T, U>(this IPagingRequest request, IQueryable<T> source, CancellationToken cancellationToken = default) where U : IPagingResponse<T>, new()
    {
        var totalRecords = await source.CountAsync();

        var results = await source
            .Skip(request.RecordsPerPage * request.PageIndex)
            .Take(request.RecordsPerPage)
            .ToListAsync(cancellationToken);

        //int totalRecords = source.TryGetNonEnumeratedCount(out int total) ? total : source.Count();

        int resultsCount = results.Count;

        return new U
        {
            PageIndex = request.PageIndex,
            RecordsPerPage = request.RecordsPerPage,
            RecordCount = resultsCount,
            TotalRecords = totalRecords,
            TotalPages = (totalRecords + request.RecordsPerPage - 1) / request.RecordsPerPage,
            Results = results
        };
    }

    #endregion IPagingRequest
}