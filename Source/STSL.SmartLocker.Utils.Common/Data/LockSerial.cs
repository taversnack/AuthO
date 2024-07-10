namespace STSL.SmartLocker.Utils.Common.Data;

public readonly record struct LockSerial(int Value)
{
    // NOTE: [0] Beware implicit operator, this is not a dangerous case though due to covariance
    // of LockSerial as an int
    public static implicit operator int(LockSerial serial) => serial.Value;
}