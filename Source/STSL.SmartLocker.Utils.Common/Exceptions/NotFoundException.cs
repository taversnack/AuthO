using Microsoft.AspNetCore.Http;

namespace STSL.SmartLocker.Utils.Common.Exceptions;

public sealed class NotFoundException : BaseException
{
    public override string Title => "Not Found";

    public override int? Status => StatusCodes.Status404NotFound;

    public NotFoundException(string detail) => Detail = detail;
    public NotFoundException(Guid id, string entityTypeName = "entity")
        => Detail = $"No {entityTypeName} with matching Id of {id}";
}
