using System;

namespace STSL.SmartLocker.Utils.Kiosk.Printer.Evolis.Contracts;

public interface IEncodeDecode
{
    public event EventHandler<string> OnMessageRecieved;
    void Run();
    string? GetCardCSN();
    string? GetCardHID();
}

