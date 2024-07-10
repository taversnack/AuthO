using System.Data.Common;

namespace STSL.SmartLocker.Utils.Common.Exceptions;

// Probably move this to Data project?
public interface IDatabaseExceptionHandler
{
    void HandleException(DbException exception);
}