using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

public interface IFilteredRequest
{
    string[]? FilterProperties { get; init; }
    string FilterValue { get; init; }
    bool StartsWith { get; init; }
}

public interface ISortedRequest
{
    string? SortBy { get; init; }
    SortOrder SortOrder { get; init; }
}

public interface IPagingRequest
{
    int PageIndex { get; init; }
    int RecordsPerPage { get; init; }
}

public interface IPagingResponse<T>
{
    int PageIndex { get; init; } // 0 based
    int RecordsPerPage { get; init; }
    int RecordCount { get; init; }
    int TotalRecords { get; init; }
    int TotalPages { get; init; }
    IReadOnlyCollection<T> Results { get; init; }
}