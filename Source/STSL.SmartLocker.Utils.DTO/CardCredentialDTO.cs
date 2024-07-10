using STSL.SmartLocker.Utils.Common.Data;

namespace STSL.SmartLocker.Utils.DTO;

public sealed record class CreateCardCredentialDTO(
    string? SerialNumber,
    string HidNumber,
    CardType CardType,
    string? CardLabel,
    Guid? CardHolderId);

public sealed record class CardCredentialDTO(
    Guid Id,
    string? SerialNumber,
    string HidNumber,
    CardType CardType,
    string? CardLabel,
    Guid? CardHolderId);

public sealed record class UpdateCardCredentialDTO(
    string? SerialNumber,
    string HidNumber,
    CardType CardType,
    string? CardLabel,
    Guid? CardHolderId);