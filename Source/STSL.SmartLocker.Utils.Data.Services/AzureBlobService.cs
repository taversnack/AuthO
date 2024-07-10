using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using STSL.SmartLocker.Utils.Data.Services.Contracts;
using STSL.SmartLocker.Utils.DTO;
using Azure.Storage.Sas;

namespace STSL.SmartLocker.Utils.Data.Services;

internal class AzureBlobService : IAzureBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobService(BlobServiceClient blobServiceClient) => (_blobServiceClient) = (blobServiceClient);

    #region Azure Operations

    public async Task<string> CreateBlobAsync(CreateAzureBlobDTO blob, string containerName)
    {
        var blobName = GenerateUniqueBlobName(blob.FileName);
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        using var fileMemoryStream = new MemoryStream(blob.BlobData, writable: false);
        var headers = new BlobHttpHeaders
        {
            ContentType = blob.BlobContentType,
        };

        await blobClient.UploadAsync(fileMemoryStream, new BlobUploadOptions { HttpHeaders = headers });

        return blobName;
    }

    public async Task DeleteBlobAsync(string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync())
        {
            await blobClient.DeleteAsync();
        }
        else
        {
            throw new InvalidOperationException("Blob not found");
        }
    }

    public string GetBlobSasUri(string blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (blobClient.CanGenerateSasUri)
        {
            // define read-only permissions
            var permissions = BlobSasPermissions.Read;

            // token params
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerClient.Name,
                BlobName = blobName,
                Resource = "b", // blobs
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1),
                Protocol = SasProtocol.Https
            };

            sasBuilder.SetPermissions(permissions);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.ToString();
        }
        else
        {
            throw new InvalidOperationException("Cannot generate a SAS token.");
        }
    }

    private static string GenerateUniqueBlobName(string fileName) => new(Guid.NewGuid().ToString() + Path.GetExtension(fileName));

    #endregion Azure Operations
}
