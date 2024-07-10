using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.BlubugConfigProducer.DTO;

// NOTE: Might be nice to strongly type SerialNumber again..
// Would be good to know that every CardSerial is a valid serial,
// constructor should throw if regex is not met (16 hex chars).
// Much of this code already exists in git history, just need to stop
// being lazy and add the nullable supporting code, parse overrides, toString etc.
public readonly record struct LockConfigCardDTO(string SerialNumber, CardType CardType = CardType.User);
