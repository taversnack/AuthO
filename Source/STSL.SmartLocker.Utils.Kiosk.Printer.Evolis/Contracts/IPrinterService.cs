using STSL.SmartLocker.Utils.Kiosk.Models;

namespace STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;

public interface IPrinterService
{
    Response DispenseNewCard();
    Response ReturnAndProcessTemporaryCard();
    Response PrepareCardForEncoding();
    Response ReadCardData();
    Response EjectCardOrMoveToBack(string action);
    Response RejectCard();
}