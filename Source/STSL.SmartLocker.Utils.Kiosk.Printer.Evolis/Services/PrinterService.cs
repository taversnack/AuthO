using Evolis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using STSL.SmartLocker.Utils.Common.Enum;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.Data.Services.Contracts.Kiosk;
using STSL.SmartLocker.Utils.DTO;
using STSL.SmartLocker.Utils.Kiosk.Models;
using STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;


namespace STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Services;

public class PrinterService : IPrinterService
{
    private readonly ILogger<PrinterService> _logger;
    private readonly IEncodeDecode _encodeDecode;
    private readonly PrinterSettings _printerSettings;
    private readonly IEmailService _emailService;
    private readonly IPostMessageService _postMessageService;
    private bool encoderRunning = false;
    private bool cardRequired = false;
    private Thread cardEjectionThread;

    public PrinterService(ILogger<PrinterService> logger, IEncodeDecode encodeDecode,
        IOptions<PrinterSettings> printerSettings, IEmailService emailService,
        IPostMessageService postMessageService)
    {
        _logger = logger;
        _encodeDecode = encodeDecode;
        _printerSettings = printerSettings.Value;
        _postMessageService = postMessageService;
        cardEjectionThread = new System.Threading.Thread(WatchForCardInsertionAndEject);
        cardEjectionThread.Start();
        _emailService = emailService;
    }

    public void SetCardRequirement(bool requirement)
    {
        cardRequired = requirement;
    }

    public Response DispenseNewCard()
    {
        SetCardRequirement(true);
        Connection co = new Connection(_printerSettings.PrinterName);
        try
        {
            if (!EjectCard(co))
            {
                _logger.LogError("Failed to eject card.");
                return new Response { Success = false, Message = "Invalid Card" };
            }

            _logger.LogInformation("CardDispensedSuccessfully");

            Status printerStatus;
            ReturnCode returnCode;

            if (!co.GetStatus(out printerStatus, out returnCode))
            {
                CheckPrinterErrors(printerStatus);
            }
            else
            {
                _logger.LogInformation($"Printer Status: {returnCode}");
            }

            MonitorFeederCapacityAndAlert();
            return new Response { Success = true, Message = "Card Dispensed Successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in DispenseCard: {ex.Message}");
            return new Response { Success = false, Message = $"> Error: {ex.Message}" };
        }
        finally
        {
            co.Close();
            SetCardRequirement(false);
        }
    }

    public Response ReturnAndProcessTemporaryCard()
    {
        SetCardRequirement(true);
        Connection co = new Connection(_printerSettings.PrinterName);
        try
        {
            if (!VerifyPrinterConnection(co))
            {
                return new Response { Success = false, Message = "Error: can't open printer context." };
            }

            Status printerStatus;
            ReturnCode returnCode;
            if (!co.GetStatus(out printerStatus, out returnCode))
            {
                CheckPrinterErrors(printerStatus);
            }
            else
            {
                _logger.LogInformation($"Printer Status: {returnCode}");
            }

            if (!PrepareBezel(co))
            {
                return new Response { Success = false, Message = "Failed to prepare bezel." };
            }

            if (!MoveToPrintPosition(co))
            {
                return new Response { Success = false, Message = "Failed to move to print position." };
            }

            // Read card from encoder
            var readResult = ReadCardData();
            if (!readResult.Success)
            {
                // If encoder rejects the card, eject it back to user (Se)
                if (!EjectCard(co))
                {
                    return new Response { Success = false, Message = "Failed to eject card." };
                }
                return new Response { Success = false, Message = "Card Rejected ByEncoder" };
            }
            SetCardRequirement(false);
            return new Response { Data = readResult.Data, Success = true, Message = "Card Returned Successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return new Response { Success = false, Message = $"> Error: {ex.Message}" };
        }
        finally
        {
            _logger.LogInformation("ReturnTemporaryCard called successfully.");
            co.Close();
            SetCardRequirement(false);
        }
    }

    Response InitializeCardForPrinting(Connection co)
    {
        try
        {
            if (!VerifyPrinterConnection(co))
            {
                return new Response { Success = false, Message = "> Error: can't open printer context." };
            }

            var dispenseFeeder = co.SendCommand("Pcim;F"); // EPSDK_IT_Feeder
            if (dispenseFeeder == null)
            {
                return new Response { Success = false, Message = $"> Error: {co.GetLastError()}" };
            }

            if (!MoveToContactlessPosition(co))
            {
                _logger.LogError("Failed to move to contactless position.");
                return new Response { Success = false, Message = "Failed to move to contactless position." };
            }

            _logger.LogInformation("Card primed successfully.");
            return new Response { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return new Response { Success = false, Message = $"> Error: {ex.Message}" };
        }
    }

    #region Encoder
    public Response PrepareCardForEncoding()
    {
        SetCardRequirement(true);
        Connection co = new Connection(_printerSettings.PrinterName);
        try
        {
            // Prime the card
            var primeResult = InitializeCardForPrinting(co);
            if (!primeResult.Success)
            {
                _logger.LogError($"Priming card failed : {primeResult.Message}");
                co.Close();
                return new Response { Success = false, Message = primeResult.Message };
            }
            // Read the card
            var readResult = ReadCardData();
            if (!readResult.Success)
            {
                _logger.LogError($"Reading card failed: {readResult.Message}");
                RejectCard();
                co.Close();
                return new Response { Success = false, Message = readResult.Message };
            }

            _logger.LogInformation("Card is ready for encoding.");
            return new Response { Success = true, Data = readResult.Data };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return new Response { Success = false, Message = $"> Error: {ex.Message}" };
        }
        finally
        {
            co?.Close();
            SetCardRequirement(false);
        }
    }

    public Response EjectCardOrMoveToBack(string action)
    {
        try
        {
            SetCardRequirement(true);
            if (action == "Eject")
            {
                Connection co = new Connection(_printerSettings.PrinterName);
                if (!EjectCard(co))
                {
                    return new Response { Success = false, Message = "Failed to eject card." };
                }
                return new Response { Success = true, Message = "Card ejected successfully." };
            }
            else if (action == "Move")
            {
                Connection co = new Connection(_printerSettings.PrinterName);
                if (!SuccessfulMoveToBack(co))
                {
                    return new Response { Success = false, Message = "Failed to move card to back of machine." };
                }
                return new Response { Success = true, Message = "Moved to the back successfully" };
            }
            return new Response { Success = false, Message = "Invalid action." };
        }
        finally
        {
            SetCardRequirement(false);
        }

    }

    public Response ReadCardData()
    {
        string? hidNumber = null;
        string? serialNumber = null;
        if (!encoderRunning)
        {
            _encodeDecode.Run();
            System.Threading.Thread.Sleep(3000);
            encoderRunning = true;
        }
        hidNumber = _encodeDecode.GetCardHID();
        serialNumber = _encodeDecode.GetCardCSN();
        if (serialNumber != null && serialNumber.StartsWith("0001802004"))
        {
            serialNumber = serialNumber.Substring(10).PadRight(16, '0');
        }
        else
        {
            serialNumber = null;
        }
        if (string.IsNullOrWhiteSpace(hidNumber) && string.IsNullOrWhiteSpace(serialNumber))
        {
            _logger.LogError("Blank card detected. Unable to read card details.");
            return new Response { Success = false, Message = "Blank card detected. Unable to read card details." };
        }
        if ((serialNumber == null) || (hidNumber == null))
        {
            _logger.LogError("Could not get card serialnumber or hidnumber, please verify the card is of the correct type");
            return new Response { Success = false, Message = "Could not get serial or hid!" };
        }
        return new Response { Success = true, Data = new { HidNumber = hidNumber, SerialNumber = serialNumber } };
    }
    #endregion

    #region Private Methods


    private void CheckPrinterErrors(Status status)
    {
        // Ribbon related errors
        if (status.IsOn(Status.ErrFlag.ERR_RIBBON_ERROR) || status.IsOn(Status.ErrFlag.ERR_BAD_RIBBON) || status.IsOn(Status.ErrFlag.ERR_RIBBON_ENDED))
        {
            _postMessageService.PostMessage(new Response
            {
                Message = ErrorType.Ribbon.ToString(),
                Data = null,
                Handler = "PrinterError",
                Success = false
            });
        }

        // Reject box related errors
        if (status.IsOn(Status.ErrFlag.ERR_REJECT_BOX_FULL) || status.IsOn(Status.ErrFlag.ERR_REJECT_BOX_COVER_OPEN))
        {
            _postMessageService.PostMessage(new Response
            {
                Message = ErrorType.RejectBox.ToString(),
                Data = null,
                Handler = "PrinterError",
                Success = false
            });
        }

        // Mechanical errors
        if (status.IsOn(Status.ErrFlag.ERR_MECHANICAL))
        {
            _logger.LogError("Mechanical error detected.");
            _postMessageService.PostMessage(new Response
            {
                Message = ErrorType.Mechanical.ToString(),
                Data = null,
                Handler = "PrinterError",
                Success = false
            });
        }

        // Temperature errors
        if (status.IsOn(Status.ErrFlag.ERR_HEAD_TEMP) || status.IsOn(Status.ErrFlag.ERR_RET_TEMPERATURE))
        {
            _logger.LogError("Temperature error detected.");
            _postMessageService.PostMessage(new Response
            {
                Message = ErrorType.Temperature.ToString(),
                Data = null,
                Handler = "PrinterError",
                Success = false
            });
        }

        // Data handling errors
        if (status.IsOn(Status.ErrFlag.ERR_BLANK_TRACK) || status.IsOn(Status.ErrFlag.ERR_MAGNETIC_DATA) ||
            status.IsOn(Status.ErrFlag.ERR_READ_MAGNETIC) || status.IsOn(Status.ErrFlag.ERR_WRITE_MAGNETIC))
        {
            _logger.LogError("Data handling error detected.");
            _postMessageService.PostMessage(new Response
            {
                Message = ErrorType.DataHandling.ToString(),
                Data = null,
                Handler = "PrinterError",
                Success = false
            });
        }

        // Cover errors
        if (status.IsOn(Status.ErrFlag.ERR_COVER_OPEN))
        {
            _logger.LogError("Cover error detected.");
            _postMessageService.PostMessage(new Response { Message = ErrorType.Cover.ToString(), Data = null, Handler = "PrinterError", Success = false });
        }

        // Clear errors
        if (status.IsOn(Status.ErrFlag.ERR_CLEAR_ERROR) || status.IsOn(Status.ErrFlag.ERR_CLEAR_ENDED) || status.IsOn(Status.ErrFlag.ERR_BAD_CLEAR))
        {
            _logger.LogError("Clear error detected.");
            _postMessageService.PostMessage(new Response { Message = ErrorType.ClearError.ToString(), Data = null, Handler = "PrinterError", Success = false });
        }

    }
    
    private void WatchForCardInsertionAndEject()
    {
        Connection co = new Connection(_printerSettings.PrinterName);

        try
        {
            while (true)
            {
                if (!cardRequired && CheckIfCardIsPresent(co))
                {
                    EjectCard(co);
                }

                Status printerStatus;
                ReturnCode returnCode;
                if (!co.GetStatus(out printerStatus, out returnCode))
                {
                    CheckPrinterErrors(printerStatus);
                }
                else
                {
                    _logger.LogInformation($"Printer Status: {returnCode}");
                }

                Thread.Sleep(3000);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            co.Close();
            return;
        }
    }

    private bool CheckIfCardIsPresent(Connection co)
    {
        try
        {
            if (co.GetStatus(out Status status))
            {
                bool isCardInserted = status.IsOn(Status.InfFlag.INF_CARD_PRINT);
                return isCardInserted;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return false;
        }
    }

    private void MonitorFeederCapacityAndAlert()
    {
        bool isFeederNearEmpty = CheckForLowFeederCapacity();
        if (isFeederNearEmpty)
        {
            AlertAboutLowFeederCapacity();
        }
    }

    private bool CheckForLowFeederCapacity()
    {
        Connection co = new Connection(_printerSettings.PrinterName);
        try
        {
            var sensorReadout = co.SendCommand("Rse;n");

            _logger.LogInformation($"Original Feeder Near Empty Sensor Readout: {sensorReadout}");

            var voltageString = sensorReadout.Replace(" Volts", "").Replace(",", ".");

            if (double.TryParse(voltageString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double voltage))
            {
                _logger.LogInformation($"Feeder Near Empty Sensor Voltage: {voltage}V");

                if (voltage > 4.27)
                {
                    _logger.LogInformation("Feeder is near empty.");
                    return true;
                }
                else
                {
                    _logger.LogInformation("Feeder has sufficient cards.");
                    return false;
                }
            }
            else
            {
                _logger.LogError("Failed to parse feeder near empty sensor voltage.");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking feeder near empty status: {ex.Message}");
            return false;
        }
        finally
        {
            co.Close();
        }
    }

    private void AlertAboutLowFeederCapacity()
    {
        var options = new EmailRequestDTO
        {
            HtmlContent = $"Hi, <br /><br />" +
            $"The Kiosk Printer feeder is less than 10 temporary ID cards.<br /><br />" +
            "Please reload the kiosk with more cards.<br /><br />" +
            "Regards, <br />" +
            "Locker Team",
            Subject = "Kiosk Printer Feeder Nearly Empty",
            EmailType = Common.Enum.EmailType.StatusReport
        };

        try
        {
            _emailService.SendNotificationEmailAsync(options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send feeder near empty email.");
        }
    }

    private bool VerifyPrinterConnection(Connection co)
    {

        if (!co.IsOpen())
        {
            _logger.LogError("Error: can't open printer context.");
            return false;
        }

        return true;
    }

    #endregion

    #region Private Methods DispenseCard

    private bool MoveToContactlessPosition(Connection co)
    {
        var moveToContactlessPos = co.SendCommand("Sic"); // EPSDK_CD_ContactLessPos
        if (moveToContactlessPos == null)
        {
            _logger.LogError($"Error: {co.GetLastError()}");
            return false;
        }

        _logger.LogInformation("Moved to contactless position successfully.");
        return true;
    }

    private bool EjectCard(Connection co)
    {
        var prepareBezelTask = Task.Run(() => co.SendCommand("Pcem;I"));

        var prepareBezel = prepareBezelTask.Result;

        if (prepareBezel != null)
        {
            _logger.LogInformation("Bezel prepared successfully.");
            var ejectCard = co.SendCommand("Se"); // EPSDK_CD_EjectCard
            if (ejectCard == null)
            {
                _logger.LogError($"Error: {co.GetLastError()}");
                return false;
            }
            return true;
        }
        _logger.LogInformation("Card ejected successfully.");
        return true;
    }
    #endregion

    #region Private ReturnTemporaryCard

    private bool PrepareBezel(Connection co)
    {
        var prepareFrontBezel = Task.Run(() => co.SendCommand("Pcim;I"));
        var completeTast = Task.WhenAny(prepareFrontBezel, Task.Delay(5000)).Result;

        if (completeTast == Task.Delay(5000))
        {
            throw new TimeoutException("Bezel preparation timed out.");
        }

        var prepareBezelTask = Task.Run(() => co.SendCommand("Pcrm;D")); // Default (= Reject box at the rear of the printer) (PRIMACY/KC Prime)l
        var feederTimeoutTask = Task.Delay(5000); // 5 seconds timeout task

        var completedTask = Task.WhenAny(prepareBezelTask, feederTimeoutTask).Result;

        if (completedTask == feederTimeoutTask)
        {
            // Feeder check timed out
            throw new TimeoutException("Feeder check timed out.");
        }

        var prepareBezel = prepareBezelTask.Result;

        if (prepareBezel != null)
        {
            _logger.LogInformation("Bezel prepared successfully.");
            return true;
        }

        _logger.LogError($"Error: {co.GetLastError()}");
        return false;
    }

    private bool MoveToPrintPosition(Connection co)
    {
        var returnCard = co.SendCommand("Sic"); // EPSDK_CardDestination for EPSDK_CD_PrintPos
        if (returnCard != null)
        {
            _logger.LogInformation("Moved to print position successfully.");
            return true;
        }

        _logger.LogError($"Error: {co.GetLastError()}");
        return false;
    }

    public Response RejectCard()
    {

        Connection co = new Connection(_printerSettings.PrinterName);
        try
        {
            var rejectCard = co.SendCommand("Ser"); // EPSDK_CardDestination for EPSDK_CD_Reject
            if (rejectCard != null)
            {
                _logger.LogInformation("Card rejected successfully.");
                return new Response(null, true, "Card rejected successfully.");
            }
            else
            {
                _logger.LogError($"Error: {co.GetLastError()}");
                return new Response(null, false, "Failed to reject card");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {ex.Message}");
            return new Response(null, false, "Error rejecting card: " + ex.Message);
        }
    }

    private bool SuccessfulMoveToBack(Connection co)
    {
        var returnCard = co.SendCommand("Ser");
        if (returnCard != null)
        {
            _logger.LogInformation("Moved to back successfully.");
            return true;
        }

        _logger.LogError($"Error: {co.GetLastError()}");
        return false;
    }

    #endregion


}
