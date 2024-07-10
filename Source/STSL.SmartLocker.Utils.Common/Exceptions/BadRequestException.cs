using Microsoft.AspNetCore.Http;

namespace STSL.SmartLocker.Utils.Common.Exceptions;

public sealed class BadRequestException : BaseException
{
    public override string Title => "Bad Request";

    public override int? Status => StatusCodes.Status400BadRequest;

    public override string Type => ProblemsNamespace + Status;

    public BadRequestException(string detail, IReadOnlyDictionary<string, string> errors)
        => (Detail, Extensions) = (detail, new Dictionary<string, object?>(new[] { KeyValuePair.Create("errors", (object?)errors) }));

    public BadRequestException(string detail, IReadOnlyDictionary<int, IReadOnlyDictionary<string, string>> errors)
        => (Detail, Extensions) = (detail, new Dictionary<string, object?>(new [] { KeyValuePair.Create("errors", (object?)errors) }));

    public BadRequestException(IReadOnlyDictionary<string, string> errors)
        => Extensions = new Dictionary<string, object?>(new[] { KeyValuePair.Create("errors", (object?)errors) });

    public BadRequestException(IReadOnlyDictionary<int, IReadOnlyDictionary<string, string>> errors)
        => Extensions = new Dictionary<string, object?>(new[] { KeyValuePair.Create("errors", (object?)errors) });

    public BadRequestException(string detail) => Detail = detail;
}
