using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;

namespace Incepted.Domain.Companies.Entities;

public class CompanyFile
{
    public Guid Id { get; private set; }
    public string FileName { get; private set; }
    public string StoredFileName { get; private set; }
    public FileType Type { get; private set; }
    public DateTimeOffset LastModified { get; private set; }
    public ContentType ContentType { get; private set; }

    public CompanyFile(Guid id, string fileName, string storedFileName, FileType type, DateTimeOffset lastModified)
    {
        if (id == Guid.Empty) throw new ArgumentException("Company file Id can't be empty", $"{nameof(CompanyFile)} {nameof(id)}");
        if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("Company file name can't be empty", $"{nameof(CompanyFile)} {nameof(fileName)}");
        if (lastModified > DateTimeOffset.Now) throw new ArgumentException("Company file last modified date can't be in the future", $"{nameof(CompanyFile)} {nameof(lastModified)}");
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
        public static CompanyFile ToEntity(FileDTO fileDTO)
        {
            return new CompanyFile(fileDTO.Id, fileDTO.FileName, fileDTO.StoredFileName, fileDTO.Type, fileDTO.LastModified);
        }

        public static FileDTO ToDTO(CompanyFile fileEntity)
        {
            return new FileDTO(fileEntity.Id, fileEntity.FileName, fileEntity.StoredFileName, fileEntity.Type, fileEntity.LastModified, fileEntity.ContentType.ToString());
        }
    }
}

public class EmptyFile : CompanyFile
{
    public EmptyFile() : base(Guid.NewGuid(), "empty.pdf", "empty.pdf", FileType.None, DateTimeOffset.MinValue)
    { }
}
