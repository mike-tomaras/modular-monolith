using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Optional;
using Serilog;
using System.Text.RegularExpressions;

namespace Incepted.Shared;

public static class FileValidations
{
    public static long allowedMaxFileSizeBytes = 100 * 1024 * 1024; //100MB
    private static int maxAllowedFileNameLength = 250;
    private static string[] allowedFileTypes = new string[] { ".pdf", ".docx", ".txt" };
    private static Dictionary<string, List<byte[]>> fileSignatures = new Dictionary<string, List<byte[]>>
                    {
                    { ".docx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
                    { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
                    { ".xlsx", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }
                };

    public static bool ValidateFile(IBrowserFile file, FileType type, List<FileUploadResult> uploadResults) =>
        ValidateFile(file.Name, file.Size, file.LastModified, file.ContentType, type, file.OpenReadStream(), uploadResults);
    public static bool ValidateFile(IFormFile file, FileType type, List<FileUploadResult> uploadResults) =>
        ValidateFile(file.FileName, 1, DateTimeOffset.Now, file.ContentType, type, file.OpenReadStream(), uploadResults);
    
    
    
    private static bool ValidateFile(string fileName, long fileSize, DateTimeOffset lastModified, string contentType, FileType type, Stream content, List<FileUploadResult> uploadResults)
    {
        var isValid = true;
        ValidateFileExtension(fileName)
            .Map(_ => ValidateFileName(fileName))
            .Map(_ => ValidateFileSize(fileSize))
            .Map(async _ => await ValidateFileSignatureAsync(content, fileName))
            .MatchNone(error =>
            {
                uploadResults.Add(
                    new FileUploadResult(
                        File: new FileDTO(Guid.Empty, fileName, string.Empty, type, lastModified, contentType),
                        Uploaded: false,
                        ErrorCode: error
                    ));

                isValid = false;
            });

        return isValid;
    }

    private static string Sanitize(this string fileName) => fileName.Replace("\0", string.Empty);
    
    private static Option<Unit, ErrorCode> ValidateFileExtension(string fileName)
    {
        if (allowedFileTypes.Contains(fileName.Sanitize().FileExtension())) return new Unit().Some<Unit, ErrorCode>();

        Log.Warning("{FileValidationError}: Attempted to upload a file with extension {FileExtension}", "Extension", fileName.Sanitize().FileExtension());

        return Option.None<Unit, ErrorCode>(FileErrorCodes.ExtensionNotAllowed);
    }

    private static Option<Unit, ErrorCode> ValidateFileName(string fileName)
    {
        if (fileName.Length > maxAllowedFileNameLength)
        {
            Log.Warning("{FileValidationError}: Attempted to upload a file with a filename longer than {FileNameMaxLength}", "Size", maxAllowedFileNameLength);
            return Option.None<Unit, ErrorCode>(FileErrorCodes.FileNameTooLong);
        }
        else if (!Regex.IsMatch(fileName, @"^[\w\-. \(\)\[\]]+$"))
        {
            Log.Warning("{FileValidationError}: Attempted to upload a file with a filename with invalid characters", "InvalidChars");
            return Option.None<Unit, ErrorCode>(FileErrorCodes.InvalidCharacters);
        }

        return new Unit().Some<Unit, ErrorCode>();
    }

    private static Option<Unit, ErrorCode> ValidateFileSize(long fileSize)
    {
        if (fileSize <= allowedMaxFileSizeBytes) return new Unit().Some<Unit, ErrorCode>();

        Log.Warning("{FileValidationError}: Attempted to upload a file with size {FileSize}", "Size", fileSize);

        return Option.None<Unit, ErrorCode>(FileErrorCodes.FileTooLarge);
    }

    private static async Task<Option<Unit, ErrorCode>> ValidateFileSignatureAsync(Stream stream, string fileName)
    {
        var signatures = fileSignatures[fileName.Sanitize().FileExtension()];
        var byteCountToRead = signatures.Max(m => m.Length);
        var headerBytes = new byte[byteCountToRead];

        await stream.ReadAsync(headerBytes, 0, byteCountToRead);
        if (!signatures.Any(signature => headerBytes.Take(signature.Length).SequenceEqual(signature)))
        {
            Log.Warning("{FileValidationError}: Attempted to upload a file with extension {FileExtension} that had the wrong signature", "Signature", fileName.Sanitize().FileExtension());
            return Option.None<Unit, ErrorCode>(FileErrorCodes.FileSignatureNotValid);
        }

        return new Unit().Some<Unit, ErrorCode>();
    }    
}
