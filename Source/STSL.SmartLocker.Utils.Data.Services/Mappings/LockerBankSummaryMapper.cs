using STSL.SmartLocker.Utils.Data.Services.Contracts.Helpers;
using STSL.SmartLocker.Utils.Data.SqlServer.Views;
using STSL.SmartLocker.Utils.DTO;

namespace STSL.SmartLocker.Utils.Data.Services.Mappings;

internal sealed class LockerBankSummaryMapper : IMapsToDTO<LockerBankSummaryDTO, LockerBankAdminSummaries_View>
{
    public static LockerBankSummaryDTO ToDTO(LockerBankAdminSummaries_View model) => new
    (
        Id: model.LockerBankId,
        Name: model.Name,
        Description: model.Description,
        Behaviour: model.Behaviour,
        LockerCount: model.TotalLockers,
        VacancyCount: model.VacantLockers,
        HasWarnings: model.HasWarnings
    );
}