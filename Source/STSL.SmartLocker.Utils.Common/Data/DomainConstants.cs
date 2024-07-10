namespace STSL.SmartLocker.Utils.Common.Data;

public static class DomainConstants
{
    // NOTE: Future changes to limitations on card credential count per locker should update
    private static readonly int maxUserCardCredentialsPerLocker = 90;
    private static readonly int maxSpecialCardCredentialsPerLocker = 10;

    public static int MaxUserCardCredentialsPerLocker => maxUserCardCredentialsPerLocker;
    public static int MaxSpecialCardCredentialsPerLocker => maxSpecialCardCredentialsPerLocker;
}
