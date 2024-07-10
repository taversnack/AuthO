using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.MSIProcessor.Services;

namespace STSL.SmartLocker.Utils.MSIProcessor;

public class ProcessMSIFunction
{
    private readonly ConsumeService _consumeService;
    private readonly ILogger<ProcessMSIFunction> _logger;

    public ProcessMSIFunction(ConsumeService consumeService, ILogger<ProcessMSIFunction> logger)
    {
        _consumeService = consumeService;
        _logger = logger;
    }

    [Function(nameof(ProcessMSIFunction))]
    public async Task Run([BlobTrigger("%StorageContainerName%/{name}", Connection = "StorageAccountConnectionString")] Stream stream, string name)
    {
        await _consumeService.ProcessMSIAsync(stream);

        _logger.LogInformation("Blob trigger function processed blob: {name}", name);
    }
}
