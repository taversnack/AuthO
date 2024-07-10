using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Messages;
using System.Data;

namespace STSL.SmartLocker.Utils.BluBugMessageConsume.Services
{
    public class DatabaseService
    {
        private readonly ILogger _logger;

        public DatabaseService(
            ILogger<DatabaseService> logger)
        {
            _logger = logger;
        }

        // returns true if a duplicate
        public async Task<bool> ProcessMessageInDatabaseAsync(BluBugMessage m)
        {
            var databaseConnectionString = Environment.GetEnvironmentVariable("SmartLockerDatabase");
            using SqlConnection connection = new SqlConnection(databaseConnectionString);

            SqlCommand cmd = new("slkmart.ProcessBluBugMessage", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            SqlParameter isDuplicateParameter = new()
            {
                ParameterName = "@isDuplicate",
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Output,
            };
            cmd.Parameters.Add(isDuplicateParameter);

            cmd.Parameters.AddRange(new SqlParameter[]
            {
                new()
                {
                    ParameterName = "@originAddress",
                    SqlDbType = SqlDbType.Int,
                    Value = m.OriginAddress
                },
                new()
                {
                    ParameterName = "@serverTimestamp",
                    SqlDbType = SqlDbType.DateTime2,
                    Value = m.ServerTimestamp
                },
                new()
                {
                    ParameterName = "@boundaryAddress",
                    SqlDbType = SqlDbType.Int,
                    Value = m.BoundaryAddress
                }
            });

            AddParameterIfValueNotNull(cmd, m.OriginTimestamp, "@originTimestamp", SqlDbType.DateTime2);
            AddParameterIfValueNotNull(cmd, m.ReadingSeqno, "@readingSeqno", SqlDbType.BigInt);
            AddParameterIfValueNotNull(cmd, m.AuditTypeCode, "@auditTypeCode", SqlDbType.TinyInt);
            AddParameterIfValueNotNull(cmd, m.AuditSubject, "@auditSubject", SqlDbType.Char, size: 16);
            AddParameterIfValueNotNull(cmd, m.AuditObject, "@auditObject", SqlDbType.Char, size: 16);
            AddParameterIfValueNotNull(cmd, m.ReadingBatteryVoltage, "@readingBatteryVoltage", SqlDbType.Decimal);
            AddParameterIfValueNotNull(cmd, m.ReadingVdd, "@readingVdd", SqlDbType.Decimal);
            AddParameterIfValueNotNull(cmd, m.OriginUrgent, "@originUrgent", SqlDbType.Bit);

            connection.Open();
            await cmd.ExecuteNonQueryAsync();

            return (bool)isDuplicateParameter.Value;
        }

        private static void AddParameterIfValueNotNull<T>(SqlCommand command, T value, string parameterName, SqlDbType sqlDbType, int size = default)
        {
            if (value is not null)
            {
                command.Parameters.Add(new()
                {
                    ParameterName = parameterName,
                    SqlDbType = sqlDbType,
                    Size = size,
                    Value = value
                });
            }
        }
    }
}
