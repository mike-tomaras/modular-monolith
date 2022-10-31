using Incepted.Db.DataModels.DealDMs;
using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.ValueTypes;
using Newtonsoft.Json;

namespace Incepted.Db.DataModels.CompanyDMs;

internal class LiveListItemDM : BaseDM
{
    [JsonProperty("companyId")] public Guid PartitionKey_CompanyId { get; set; }//partition key
    public Guid LiveDealId { get; set; }
    public string Type => "live";
    public string Name { get; set; }
    public Money EnterpriseValue { get; set; }
    public IEnumerable<AssigneeDM> Assignees { get; set; } = Enumerable.Empty<AssigneeDM>();

    public static class Factory
    {
        public static LiveListItemDM ToBrokerDataModel(LiveDeal liveDeal) =>
            new LiveListItemDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                PartitionKey_CompanyId = liveDeal.BrokerCompanyId,
                LiveDealId = liveDeal.Id,
                Name = liveDeal.Name,
                EnterpriseValue = liveDeal.EnterpriseValue,
                Assignees = liveDeal.AssigneesBroker.Select(AssigneeDM.Factory.ToDataModel)
            };

        public static LiveListItemDM ToInsurerDataModel(LiveDeal liveDeal) =>
            new LiveListItemDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                PartitionKey_CompanyId = liveDeal.InsuranceCompanyId,
                LiveDealId = liveDeal.Id,
                Name = liveDeal.Name,
                EnterpriseValue = liveDeal.EnterpriseValue,
                Assignees = liveDeal.AssigneesInsurer.Select(AssigneeDM.Factory.ToDataModel)
            };
    }
}