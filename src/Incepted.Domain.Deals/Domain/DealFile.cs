using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;

namespace Incepted.Domain.Deals.Domain;

public class DealFile
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public string StoredFileName { get; private set; }
    public FileType Type { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public ContentType ContentType { get; private set; }

    public DealFile(Guid id, string fileName, string storedFileName, FileType type, DateTimeOffset lastModified)
    {
        if (id == Guid.Empty) throw new ArgumentException("Deal file Id can't be empty", $"{nameof(DealFile)} {nameof(id)}");
        if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("Deal file name can't be empty", $"{nameof(DealFile)} {nameof(fileName)}");
        if (lastModified > DateTimeOffset.Now) throw new ArgumentException("Deal file last modified date can't be in the future", $"{nameof(DealFile)} {nameof(lastModified)}");
        if (string.IsNullOrEmpty(storedFileName)) storedFileName = Path.GetRandomFileName();
        //TODO validate content type

        Id = id;
        FileName = fileName;
        StoredFileName = storedFileName;
        Type = type;
        LastModified = lastModified;
        ContentType = new ContentType(FileName.FileExtension());
    }
    public static class Factory
    {
        public static DealFile ToEntity(FileDTO fileDTO)
        {
            return new DealFile(fileDTO.Id, fileDTO.FileName, fileDTO.StoredFileName, fileDTO.Type, fileDTO.LastModified);
        }

        public static FileDTO ToDTO(DealFile fileEntity)
        {
            return new FileDTO(fileEntity.Id, fileEntity.FileName, fileEntity.StoredFileName, fileEntity.Type, fileEntity.LastModified, fileEntity.ContentType.ToString());
        }
    }
}

public class EmptyFile : DealFile
{
    public EmptyFile() : base(Guid.NewGuid(), "empty.pdf", "empty.pdf", FileType.None, DateTimeOffset.MinValue)
    { }
}
