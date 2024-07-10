using STSL.SmartLocker.Utils.CLI.Models;

namespace STSL.SmartLocker.Utils.CLI.Services.Contracts;

internal interface ILockerService
{
    Task<LockerConfigJson?> GetLockerConfigJson(LockerSerial lockerId);
    Task<LockerConfig?> GetLockerConfig(LockerSerial lockerId);
    Task<bool> SetLockerConfig(LockerConfigJson config, LockerSerial lockerId);
    Task<bool> SetLockerConfig(LockerConfig config);
    Task<bool> SetupLockers(LockersSetup config);
}
