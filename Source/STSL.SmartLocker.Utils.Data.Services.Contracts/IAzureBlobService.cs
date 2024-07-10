using STSL.SmartLocker.Utils.DTO;
using System.Collections.Generic;

namespace STSL.SmartLocker.Utils.Data.Services.Contracts;

public interface IAzureBlobService
{
    Task<string> CreateBlobAsync(CreateAzureBlobDTO blob, string containerName);
    Task DeleteBlobAsync(string blobName, string containerName);
    string GetBlobSasUri(string blobName, string containerName);
}
