
// Type aliases
global using CardSerial = System.String;
global using LockerSerial = System.UInt64;

//global using CardSerialHexString = System.ReadOnlySpan<byte>;
//global using CardSerialHexString = System.String;


namespace STSL.SmartLocker.Utils.CLI
{
    internal static class Constants
    {
        public const string LockersHttpClient = "LockersHttpClient";
        // TODO: Check if this is within the valid range for MIFARE Card numbers?
        public const string EmptyCardString = "0000000000000000";
        public const int CardSerialHexStringMaxLength = 8;
        public const int LockerConfigCardBlockMaxCount = 5;
    }
}

