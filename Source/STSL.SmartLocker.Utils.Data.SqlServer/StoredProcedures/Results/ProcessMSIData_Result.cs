namespace STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results;

public class ProcessMSIData_Result
{
    public string Action { get; set; }

    public string? ActionNotes { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string? Email { get; set; }

    public string ObjectID { get; set; }

    public string? NewObjectID { get; set; }

    public bool IsTerminated { get; set; }

    public DateTime? TerminationDate { get; set; }
}
