using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Optional;

namespace Incepted.Domain.Deals.Application;

public interface IDealFileService
{
    Task<FileUploadResult> UploadAsync(Guid dealId, FileDTO file, Stream content);
    Task<Option<Unit, ErrorCode>> DeleteAsync(Guid dealId, string storedFileName);
    Task<Option<byte[], ErrorCode>> DownloadAsync(Guid dealId, string storedFileName, FileType type);
}
