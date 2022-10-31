using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Optional;

namespace Incepted.Domain.Companies.Application;

public interface ICompanyFileService
{
    Task<FileUploadResult> UploadAsync(Guid companyId, FileDTO file, Stream content);
    Task<Option<byte[], ErrorCode>> DownloadAsync(Guid companyId, string storedFileName, FileType type);
}
