using STSL.SmartLocker.Utils.Messages;

namespace STSL.SmartLocker.Utils.LockEvents.Contracts;

public interface ILockConfigUpdateService
{
    Task HandleUpdatesFromParsedMessageAsync(BluBugMessage message, CancellationToken cancellationToken = default);
}
