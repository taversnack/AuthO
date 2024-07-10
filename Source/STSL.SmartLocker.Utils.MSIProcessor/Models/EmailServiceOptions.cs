using Azure.Communication.Email;

namespace STSL.SmartLocker.Utils.MSIProcessor.Models;

public class EmailServiceOptions
{
    public required string SenderAddress { get; set; }

    public required List<EmailAddress> RecipientAddresses { get; set; }
}


public class EmailAddressRecipients
{
    public string Address { get; set; }
}