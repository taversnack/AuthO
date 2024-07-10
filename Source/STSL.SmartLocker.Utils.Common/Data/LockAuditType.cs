namespace STSL.SmartLocker.Utils.Common.Data;

public enum LockAuditType : int
{
    None = 0,

    // Unimportant Events
    // Reject Events
    RejectedUnregisteredUserCard = 10,
    RejectedUnregisteredSecuritySweepCard = 11,
    RejectedValidCardDuringSecuritySweep = 12,
    RejectedLockerConfiscated = 13,
    RejectedBatteryTooLow = 14,
    RejectedWelcomeCardPresentedNonUserCardFollowed = 16,
    RejectedWelcomeCardSharedLockerIsLocked = 17,
    RejectedWrongUserCardOnSharedLocker = 17,

    // Locker Failed To Open Events
    FailedToOpenByUserCard = 20,
    FailedToOpenByMasterCard = 21,
    FailedToOpenBySecuritySweepCard = 22,
    FailedToOpenByUnregisteredCardInInstallMode = 23,
    FailedToOpenSharedLockerAfterSecuritySweep = 24,
    FailedToOpenByBrokenOpen = 28,

    // Operator Error Events
    TimedOutLockedByUserCard = 30,
    TimedOutLockedByMasterCard = 31,
    TimedOutLockedBySecuritySweepCard = 32,
    TimedOutLockedByUnregisteredCardInInstallMode = 33,
    TimedOutLockedByWelcomeCard = 37,
    WelcomeCardPresentedNoUserCardFollowed = 40,

    // Important Events
    // Opening Events
    OpenedByUserCard = 50,
    OpenedByMasterCard = 51,
    OpenedBySecuritySweepCard = 52,
    OpenedByUnregisteredCardInInstallMode = 53,
    OpenedSharedLockerAfterSecuritySweep = 54,
    OpenedByBrokenOpen = 58,

    // Locking Events
    LockedByUserCard = 60,
    LockedByMasterCard = 61,
    LockedBySecuritySweepCard = 62,
    LockedByUnregisteredCardInInstallMode = 63,
    LockedByWelcomeCard = 67,

    // Other Events
    KeepAlive = 80,
    CardObservedNoFurtherAction = 81,
    TamperingOccurredRemainingAsIfLocked = 88
}
