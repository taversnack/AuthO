using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;

namespace STSL.SmartLocker.Utils.DTO;

public readonly record struct GlobalLockerSearchResultDTO(List<LocationDTO> Locations, List<LockerBankDTO> LockerBanks, IPagingResponse<LockerAndLockDTO> LockerAndLocks);