namespace STSL.SmartLocker.Utils.Common.Enum
{
    public enum ErrorType
    {
        Ribbon,          // Related to ribbon issues in the printer
        RejectBox,       // Issues related to the reject box (full or open)
        Mechanical,      // General mechanical errors
        Temperature,     // Errors related to temperature issues
        DataHandling,    // Errors related to handling data like magnetic data or blank tracks
        Cover,           // Errors related to the printer cover being open or improperly secured
        ClearError,      // Errors related to clearing operations within the printer
        Other            // A catch-all for errors that don't neatly fit into the above categories
    }
}
