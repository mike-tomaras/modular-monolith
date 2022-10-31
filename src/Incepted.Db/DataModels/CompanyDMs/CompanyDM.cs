using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Companies.Entities;
using Incepted.Shared;
using Incepted.Shared.Enums;
using Newtonsoft.Json;

namespace Incepted.Db.DataModels.CompanyDMs;

internal class CompanyDM : BaseDM
{
    [JsonProperty("companyId")] public Guid PartitionKey_CompanyId { get; set; }//partition key
    public string Type => "company";
    public string Name { get; set; }
    public CompanyType CompanyType { get; set; }
    public IEnumerable<EmployeeDM> Employees { get; set; } = Enumerable.Empty<EmployeeDM>();
    public FileDM? TsAndCs { get; set; }

    public static class Factory
    {
        public static CompanyDM ToDataModel(Company company) =>
            new CompanyDM
            {
                Version = 1,
                Id = company.Id,
                PartitionKey_CompanyId = company.Id,
                Name = company.Name,
                CompanyType = company.Type,
                Employees = company.Employees.Select(EmployeeDM.Factory.ToDataModel),
                TsAndCs = FileDM.Factory.ToDataModel(company.TsAndCs)
            };

        public static Company ToEntity(CompanyDM company) =>
            new Company(
                company.Id,
                company.Name,
                company.CompanyType,
                company.Employees.Select(EmployeeDM.Factory.ToEntity).ToImmutable(),
                company.TsAndCs == null ? new EmptyFile() : FileDM.Factory.ToEntityForCompany(company.TsAndCs)
            );
    }
}
