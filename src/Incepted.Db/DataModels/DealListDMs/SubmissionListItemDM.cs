using Incepted.Db.DataModels.DealDMs;
using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using Newtonsoft.Json;

namespace Incepted.Db.DataModels.CompanyDMs;

internal class SubmissionListItemDM : BaseDM
{
    [JsonProperty("companyId")] public Guid PartitionKey_CompanyId { get; set; }//partition key
    public Guid SubmissionId { get; set; }
    public string Type => "submission";
    public string Name { get; set; }
    public string BrokerName { get; set; }
    public string Industry { get; set; }
    public string SpaJurisdiction { get; set; }
    public bool IsSubmittedToInsurers { get; set; }
    public Money EnterpriseValue { get; set; }
    public IEnumerable<AssigneeDM> Assignees { get; set; } = Enumerable.Empty<AssigneeDM>();
    public DateTimeOffset? SubmissionDeadline { get; set; }

    public SubmissionListItemDM Update(DealSubmission submission)
    {
        return new SubmissionListItemDM
        {
            Version = Version,
            Id = Id,
            PartitionKey_CompanyId = PartitionKey_CompanyId,
            SubmissionId = SubmissionId,
            Name = submission.Name,
            BrokerName = submission.BrokerName,
            Industry = submission.Terms.Industry,
            SpaJurisdiction = submission.Terms.TargetJurisdiction.ToString(),
            IsSubmittedToInsurers = submission.Feedbacks.Any(),
            EnterpriseValue = submission.Pricing.EnterpriseValue,
            Assignees = submission.Assignees.Select(AssigneeDM.Factory.ToDataModel),
            SubmissionDeadline = submission.SubmissionDeadline
        };
    }

    public static class Factory
    {
        public static SubmissionListItemDM ToDataModel(DealSubmission submission) =>
            new SubmissionListItemDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                PartitionKey_CompanyId = submission.BrokerCompanyId,
                Name = submission.Name,
                BrokerName = submission.BrokerName,
                Industry = submission.Terms.Industry,
                SpaJurisdiction = submission.Terms.TargetJurisdiction.ToString(),
                IsSubmittedToInsurers = submission.Feedbacks.Any(),
                EnterpriseValue = submission.Pricing.EnterpriseValue,
                Assignees = submission.Assignees.Select(AssigneeDM.Factory.ToDataModel),
                SubmissionDeadline = submission.SubmissionDeadline
            };

        public static SubmissionListItemDM ToDataModel(SubmissionFeedback feedback, DealSubmission submission) =>
            new SubmissionListItemDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                SubmissionId = submission.Id,
                PartitionKey_CompanyId = feedback.InsuranceCompanyId,
                Name = submission.Name,
                BrokerName = submission.BrokerName,
                Industry = submission.Terms.Industry,
                SpaJurisdiction = submission.Terms.TargetJurisdiction.ToString(),
                IsSubmittedToInsurers = submission.Feedbacks.Any(),
                EnterpriseValue = submission.Pricing.EnterpriseValue,
                Assignees = submission.Feedbacks.Single(f => f.FeedbackId == feedback.Id).Assignees.Select(AssigneeDM.Factory.ToDataModel),
                SubmissionDeadline = submission.SubmissionDeadline
            };

        public static DealListItemDTO ToDTO(SubmissionListItemDM submission) =>
            new DealListItemDTO(
                submission.SubmissionId,
                submission.Name,
                submission.BrokerName,
                submission.Industry,
                submission.SpaJurisdiction,
                submission.IsSubmittedToInsurers,
                submission.EnterpriseValue,                
                submission.Assignees.Select(AssigneeDM.Factory.ToDTO).ToImmutable(),                
                submission.SubmissionDeadline
            );
    }
}