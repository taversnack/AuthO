using Microsoft.Data.SqlClient;
using STSL.SmartLocker.Utils.Common.Exceptions;
using System.Data.Common;

namespace STSL.SmartLocker.Utils.Data.SqlServer;

internal sealed class SqlServerExceptionHandler : IDatabaseExceptionHandler
{
    private const int InvalidDependentEntity = 547;
    private const int InvalidNull = 515;
    private const int DuplicateKey = 2601;
    private const int DuplicateOnUniqueConstraint = 2627;
    private const int IntegerOverflow = 8115;
    private const int TruncatedData = 8152;

    public void HandleException(DbException exception)
    {
        if (exception is SqlException sqlException)
        {
            throw sqlException.Number switch
            {
                InvalidDependentEntity => new BadRequestException("There is an issue with a dependent entity, perhaps it no longer exists"),
                InvalidNull => new BadRequestException("A null value was provided for a non nullable property"),
                DuplicateKey => new BadRequestException("A duplicate primary key meant the entity was not created"),
                DuplicateOnUniqueConstraint => new BadRequestException("A non unique value was provided for a property requiring uniqueness"),
                IntegerOverflow => new BadRequestException("An integer or binary overflow occurred, try using a smaller number"),
                TruncatedData => new BadRequestException("A string or binary value is too long and would be truncated, try using a shorter string"),
                _ => new Exception("An unknown error occurred")
            };
        }
    }
}
