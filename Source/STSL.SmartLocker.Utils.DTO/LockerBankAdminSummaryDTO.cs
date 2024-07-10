using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public sealed record LockerBankAdminSummaryDTO(Guid LocationId, string LocationName, string LocationDescription, List<LockerBankSummaryDTO> LockerBankSummaries);


// TODO: Add "usage" to this DTO, when we implement metrics for locker banks 
public sealed record LockerBankSummaryDTO(Guid Id, string Name, string Description, LockerBankBehaviour Behaviour, int LockerCount, int VacancyCount, bool HasWarnings);