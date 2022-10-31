using Incepted.Shared.Enums;
using System.Text.Json.Serialization;

namespace Incepted.Shared.DTOs;

public record class FileDTO(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("fileName")] string FileName,
    [property: JsonPropertyName("storedFileName")] string StoredFileName,
    [property: JsonPropertyName("type")] FileType Type,
    [property: JsonPropertyName("lastModified")] DateTimeOffset LastModified,
    [property: JsonPropertyName("contentType")] string ContentType);

public record class FileUploadResult(
    [property: JsonPropertyName("file")] FileDTO File,
    [property: JsonPropertyName("uploaded")] bool Uploaded,
    [property: JsonPropertyName("errorCode")] ErrorCode ErrorCode
    );

public record class FileDownloadResult(
    [property: JsonPropertyName("file")] FileDTO File,
    [property: JsonPropertyName("uploaded")] byte[] Content
    );