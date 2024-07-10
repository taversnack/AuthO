using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures;
using STSL.SmartLocker.Utils.Data.SqlServer.StoredProcedures.Results;
using STSL.SmartLocker.Utils.MSIProcessor.DTO;
using System.Data;

namespace STSL.SmartLocker.Utils.MSIProcessor.Services;

public class DatabaseService
{
    private readonly StoredProcedureRepository _storedProcedures;

    public DatabaseService(
        StoredProcedureRepository storedProcedures)
    {
        _storedProcedures = storedProcedures;
    }

    public async Task<IReadOnlyList<MSIOutputDTO>> ProcessJSONInDatabaseAsync(string json)
    {
        var tenantId = Guid.Parse(Environment.GetEnvironmentVariable("TenantId"));

        var results = await _storedProcedures.SP_ProcessMSIData(tenantId, json);

        var mappedResults = results.Select(MapMSIDataResultToDTO).ToList();

        return mappedResults;
    }

    private static MSIOutputDTO MapMSIDataResultToDTO(ProcessMSIData_Result msiOutputRecord)
        => new()
        {
            Problem = msiOutputRecord.Action,
            ProblemDetails = msiOutputRecord.ActionNotes,
            FirstName = msiOutputRecord.FirstName,
            LastName = msiOutputRecord.LastName,
            Email = msiOutputRecord.Email,
            ObjectID = msiOutputRecord.ObjectID,
            NewObjectID = msiOutputRecord.NewObjectID,
            IsTerminated = msiOutputRecord.IsTerminated,
            TerminationDate = msiOutputRecord.TerminationDate
        };
}
