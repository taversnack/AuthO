using CsvHelper;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.MSIProcessor.DTO;
using System.Globalization;
using System.Text.Json;

namespace STSL.SmartLocker.Utils.MSIProcessor.Services;

public class ConsumeService
{
    private readonly ILogger<ConsumeService> _logger;
    private readonly DatabaseService _databaseService;
    private readonly EmailService _emailService;

    public ConsumeService(
        ILogger<ConsumeService> logger,
        DatabaseService databaseService,
        EmailService emailService)
    {
        _logger = logger;
        _databaseService = databaseService;
        _emailService = emailService;
    }

    public async Task ProcessMSIAsync(Stream csvStream)
    {
        try
        {
            // Read CSV & Convert to JSON
            var json = ParseCSVAndGetJSON(csvStream);

            // Process JSON
            var result = await _databaseService.ProcessJSONInDatabaseAsync(json);

            if (result is null || result.Count == 0)
            {
                return;
            }

            // Convert result to Byte array
            var resultArray = ConvertResultToByteArray(result);

            // Send Email with attachment data
            await _emailService.SendEmailAsync(resultArray);

        } catch (Exception ex)
        {
            _logger.LogError(ex, "Exception thrown in ProcessMSIAsync: '{exception}' whilst processing MSI", ex.Message);
        }
    }

    private static string ParseCSVAndGetJSON(Stream data)
    {
        using var reader = new StreamReader(data);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Parse CSV
        var records = csv.GetRecords<MSIRecordDTO>();

        return JSONify(records);
    }

    private static byte[] ConvertResultToByteArray(IReadOnlyList<MSIOutputDTO> records)
    {
        var memoryStream = new MemoryStream();

        using var writer = new StreamWriter(memoryStream, leaveOpen: true);
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(records);
        }
        
        // Reset position for reading
        memoryStream.Seek(0, SeekOrigin.Begin); 

        return memoryStream.ToArray();
    }

    private static string JSONify(IEnumerable<MSIRecordDTO> records) => JsonSerializer.Serialize(records);
}
