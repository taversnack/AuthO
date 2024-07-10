using Microsoft.AspNetCore.Http;

namespace STSL.SmartLocker.Utils.Common.Exceptions;

public sealed class ForbiddenException : BaseException
{
    private const string DefaultDetailMessage = "You are not authorized to access this resource";

    public override string Title => "Forbidden";

    public override int? Status => StatusCodes.Status403Forbidden;

    public ForbiddenException(string detail = DefaultDetailMessage) => Detail = detail;
}
