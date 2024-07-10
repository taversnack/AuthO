using Microsoft.AspNetCore.Http;

namespace STSL.SmartLocker.Utils.Common.Exceptions;

public sealed class ConflictException : BaseException
{
    public override string Title => "Conflict";

    public override int? Status => StatusCodes.Status409Conflict;

    public ConflictException(string detail) => Detail = detail;
}
