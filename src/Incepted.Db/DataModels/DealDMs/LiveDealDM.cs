using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.ValueTypes;
using Newtonsoft.Json;

namespace Incepted.Db.DataModels.DealDMs;

internal class LiveDealDM : BaseDM
{
    [JsonProperty("dealId")] public Guid PartitionKey_DealId { get; set; }//partition key
    public string Type => "live";
    public string Name { get; set; }
    public Guid InsuranceCompanyId { get; set; }
    public Guid FeedbackId { get; set; }
    public string InsuranceCompanyName { get; set; }
    public Guid BrokerCompanyId { get; set; }
    public Guid SubmissionId { get; set; }
    public string BrokerCompanyName { get; set; }
    public IEnumerable<AssigneeDM> AssigneesBroker { get; set; } = Enumerable.Empty<AssigneeDM>();
    public IEnumerable<AssigneeDM> AssigneesInsurer { get; set; } = Enumerable.Empty<AssigneeDM>();
    public Money EnterpriseValue { get; set; }

    public static class Factory
    {
        public static LiveDealDM ToDataModel(LiveDeal deal) =>
            new LiveDealDM
            {
                Version = 1,
                Id = deal.Id,
                PartitionKey_DealId = deal.Id,
                Name = deal.Name,
                InsuranceCompanyId = deal.InsuranceCompanyId,
                FeedbackId = deal.FeedbackId,
                InsuranceCompanyName = deal.InsurerName,
                BrokerCompanyId = deal.BrokerCompanyId,
                SubmissionId = deal.SubmissionId,
                BrokerCompanyName = deal.BrokerName,
                AssigneesBroker = deal.AssigneesBroker.Select(AssigneeDM.Factory.ToDataModel),
                AssigneesInsurer = deal.AssigneesInsurer.Select(AssigneeDM.Factory.ToDataModel),
                EnterpriseValue = deal.EnterpriseValue
            };

        //public static LiveDeal ToEntity(LiveDealDM deal) =>
        //    new LiveDeal(
        //        id: deal.Id,
        //        name: deal.Name,
        //        brokerName: deal.BrokerCompanyName,
        //        brokerCompanyId: deal.BrokerCompanyId,
        //        insurerName: deal.InsuranceCompanyName,
        //        insuranceCompanyId: deal.InsuranceCompanyId
        //        );
    }
}

