using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.Enums;

namespace Incepted.Db.DataModels.SharedDMs;

internal class FileDM : BaseDM
{
    public string FileName { get; set; }
    public string StoredFileName { get; set; }
    public FileType FileType { get; set; }
    public DateTimeOffset LastModified { get; set; }

    public static class Factory
    {
        public static FileDM ToDataModel(CompanyFile file) =>
            new FileDM
            {
                Version = 1,
                Id = file.Id,
                FileName = file.FileName,
                StoredFileName = file.StoredFileName,
                FileType = file.Type,
                LastModified = file.LastModified
            };
        public static FileDM ToDataModel(DealFile file) =>
            new FileDM
            {
                Version = 1,
                Id = file.Id,
                FileName = file.FileName,
                StoredFileName = file.StoredFileName,
                FileType = file.Type,
                LastModified = file.LastModified
            };

        public static CompanyFile ToEntityForCompany(FileDM file) =>
            new CompanyFile(
                file.Id,
                file.FileName,
                file.StoredFileName,
                file.FileType,
                file.LastModified
            );
        public static DealFile ToEntityForDeal(FileDM file) =>
            new DealFile(
                file.Id,
                file.FileName,
                file.StoredFileName,
                file.FileType,
                file.LastModified
            );
    }
}
