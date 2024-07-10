using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.Data.SqlServer.Views;

public class LockerBankAdminSummaries_View
{
    public Guid LockerBankId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public LockerBankBehaviour Behaviour { get; set; } 

    public int TotalLockers { get; set; }

    public int VacantLockers { get; set; }

    public bool HasWarnings { get; set; }
}
