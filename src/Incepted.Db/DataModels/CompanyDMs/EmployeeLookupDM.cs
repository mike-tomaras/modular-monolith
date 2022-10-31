using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Companies.Entities;
using Newtonsoft.Json;

namespace Incepted.Db.DataModels.CompanyDMs;

internal class EmployeeLookupDM : BaseDM
{
    [JsonProperty("companyId")] public Guid PartitionKey_CompanyId { get; set; }//partition key
    public string Type => "employee-lookup";
    public string UserId { get; set; }

    public static class Factory
    {
        public static EmployeeLookupDM ToDataModel(Employee employee, Guid companyId) =>
            new EmployeeLookupDM
            {
                Version = 1,
                Id = employee.Id,
                PartitionKey_CompanyId = companyId,
                UserId = employee.UserId.ToString()
            };
    }
}
