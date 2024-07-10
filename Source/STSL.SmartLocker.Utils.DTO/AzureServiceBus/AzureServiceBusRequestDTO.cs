namespace STSL.SmartLocker.Utils.DTO.AzureServiceBus;

public abstract class AzureServiceBusRequestMessage
{
    public string? MessageType { get; protected set; }
}

public record AzureServiceBusCardCredentials(decimal HidNumber, long CSN);

public sealed class AzureServiceBusAuthenticationDTO : AzureServiceBusRequestMessage
{
    public AzureServiceBusAuthenticationDTO()
    {
        MessageType = "Authenticate";
    }
}

public sealed class AzureServiceBusCreateTemporaryCardDTO : AzureServiceBusRequestMessage
{
    public required int CardHolderId { get; set; } // ObjectId in C-Cure

    public required AzureServiceBusCardCredentials CurrentCardCredentials { get; set; }

    public required AzureServiceBusCardCredentials TemporaryCardCredentials { get; set; }

    public AzureServiceBusCreateTemporaryCardDTO()
    {
        MessageType = "Create";
    }
}

public sealed class AzureServiceBusReturnTemporaryCardDTO : AzureServiceBusRequestMessage
{
    public required AzureServiceBusCardCredentials TemporaryCardCredentials { get; set; }

    public AzureServiceBusReturnTemporaryCardDTO()
    {
        MessageType = "Return";
    }
}

public sealed class AzureServiceBusGetCardCredentialDTO : AzureServiceBusRequestMessage
{
    public required string CardHolderId { get; set; } // ObjectId in C-Cure

    public AzureServiceBusGetCardCredentialDTO()
    {
        MessageType = "GetCardCredential";
    }
}