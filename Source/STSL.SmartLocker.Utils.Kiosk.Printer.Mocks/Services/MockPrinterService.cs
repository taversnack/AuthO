using Microsoft.Extensions.Logging;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;

namespace STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Mock.Services;

public class MockPrinterService : IPrinterService
{
    private readonly ILogger<MockPrinterService> _logger;
    private readonly Random _random;
    private readonly PrinterState _printerState;

    public MockPrinterService(ILogger<MockPrinterService> logger)
    {
        _logger = logger;
        _random = new Random();
        _printerState = new PrinterState();
    }

    public Response DispenseNewCard()
    {
        if (SimulateError())
        {
            _logger.LogInformation("Mock: Simulating card ejection failure.");
            return new Response { Success = false, Message = "Failed to eject card (Mock)" };
        }
        else
        {
            _logger.LogInformation("Mock: Dispensing new card successfully.");
            _printerState.CardDispensed = true;
            return new Response { Success = true, Message = "Card Dispensed Successfully (Mock)" };
        }
    }

    public Response ReturnAndProcessTemporaryCard()
    {
        _logger.LogInformation("Mock: Returning and processing temporary card.");
        return new Response { Success = true, Message = "Card Returned Successfully (Mock)", Data = "Mock Data" };
    }

    public Response PrepareCardForEncoding()
    {
        if (!_printerState.CardDispensed)
        {
            _logger.LogInformation("Mock: No card to prepare for encoding.");
            return new Response { Success = false, Message = "No card to prepare for encoding (Mock)" };
        }

        _logger.LogInformation("Mock: Preparing card for encoding.");
        _printerState.CardPreparedForEncoding = true;
        return new Response { Success = true, Data = "Mock Data Ready for Encoding" };
    }

    public Response EjectCardOrMoveToBack(string action)
    {
        _logger.LogInformation($"Mock: Action {action} performed on card.");
        _printerState.Reset();
        return new Response { Success = true, Message = $"Card {action} Successfully (Mock)" };
    }

    public Response ReadCardData()
    {
        _logger.LogInformation("Mock: Reading card data.");
        return new Response { Success = true, Data = new { HidNumber = "MockHID", SerialNumber = "MockSerial" } };
    }

    public Response RejectCard()
    {
        _logger.LogInformation("Mock: Card rejected successfully.");
        return Response.Sucessfull("Card rejected successfully.");
    }

    private bool SimulateError()
    {
        // Simulate a 20% chance of error
        return _random.Next(100) < 20;
    }

    private class PrinterState
    {
        public bool CardDispensed { get; set; }
        public bool CardPreparedForEncoding { get; set; }

        public void Reset()
        {
            CardDispensed = false;
            CardPreparedForEncoding = false;
        }
    }
}





