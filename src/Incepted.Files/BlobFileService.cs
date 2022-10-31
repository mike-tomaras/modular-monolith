using Azure.Storage.Blobs;
using Incepted.Domain.Companies.Application;
using Incepted.Domain.Deals.Application;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Optional;
using Serilog;

namespace Incepted.Files.AzStorage;

public class BlobFileService : ICompanyFileService, IDealFileService
{
    private BlobServiceClient _blobServiceClient;

    public BlobFileService(string connectionString)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    private string ContainerName(FileType type) => type == FileType.InsurerTCs ? "company-files" : "submission-files";

    public async Task<FileUploadResult> UploadAsync(Guid id, FileDTO file, Stream content)
    {
        var uploadResult = new FileUploadResult(file, false, EmptyErrorCode.Empty);

        try
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName(file.Type));
            await containerClient.CreateIfNotExistsAsync();

            string fileName = Path.Combine(id.ToString(), file.StoredFileName);

            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            // Upload data from the stream
            await blobClient.UploadAsync(content, overwrite: true);

            uploadResult = uploadResult with { Uploaded = true };
        }
        catch (Exception e)
        {
            var errorIdDetails = file.Type == FileType.InsurerTCs ? "Company Id is {CompanyId}" : "Deal Id is {DealId}";
            Log.Error("Failed to upload file to Azure BLOB Storage. " + errorIdDetails + ". File name is {FileName}. Error is {Error}.", id, file.FileName, e.Message);
            uploadResult = uploadResult with { ErrorCode = FileErrorCodes.RemoteStorageFailure_Save };
        }
        
        return await Task.FromResult(uploadResult);
    }

    public async Task<Option<Unit, ErrorCode>> DeleteAsync(Guid dealId, string storedFileName)
    {
        try
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient("submission-files");
            var containerExists = await containerClient.ExistsAsync();
            if (!containerExists) return await Task.FromResult(Option.None<Unit, ErrorCode>(FileErrorCodes.BlobContainerNotFound));

            string fileName = Path.Combine(dealId.ToString(), storedFileName);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }
        catch (Exception e)
        {
            Log.Error("Failed to delete file from Azure BLOB Storage. Deal Id is {DealId}. Stored file name is {StoredFileName}. Error is {Error}.", dealId, storedFileName, e.Message);
            return await Task.FromResult(Option.None<Unit, ErrorCode>(FileErrorCodes.RemoteStorageFailure_Delete));
        }

        return await Task.FromResult(new Unit().Some<Unit, ErrorCode>());
    }

    public async Task<Option<byte[], ErrorCode>> DownloadAsync(Guid id, string storedFileName, FileType type)
    {
        try
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName(type));
            var containerExists = await containerClient.ExistsAsync();
            if (!containerExists) return await Task.FromResult(Option.None<byte[], ErrorCode>(FileErrorCodes.BlobContainerNotFound));

            string fileName = Path.Combine(id.ToString(), storedFileName);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            return (await blobClient.DownloadContentAsync())
                .Value.Content.ToArray()
                .Some<byte[], ErrorCode>();
        }
        catch (Exception e)
        {
            var errorIdDetails = type == FileType.InsurerTCs ? "Company Id is {CompanyId}" : "Deal Id is {DealId}";
            Log.Error("Failed to download file from Azure BLOB Storage. " + errorIdDetails + ". Stored file name is {StoredFileName}. Error is {Error}.", id, storedFileName, e.Message);
            return await Task.FromResult(Option.None<byte[], ErrorCode>(FileErrorCodes.RemoteStorageFailure_Download));
        }        
    }
}


