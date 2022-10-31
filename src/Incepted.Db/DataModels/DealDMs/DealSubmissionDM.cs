using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.ValueTypes;
using Newtonsoft.Json;

namespace Incepted.Db.DataModels.DealDMs;

internal class DealSubmissionDM : BaseDM
{
    [JsonProperty("dealId")] public Guid PartitionKey_DealId { get; set; }//partition key
    public string Type => "submission";
    public string Name { get; set; }
    public string BrokerName { get; set; }
    public Guid BrokerCompanyId { get; set; }
    public BasicTerms Terms { get; set; }
    public SubmissionPricing Pricing { get; set; }
    public IEnumerable<Enhancement> Enhancements { get; set; } = Enumerable.Empty<Enhancement>();
    public IEnumerable<Warranty> Warranties { get; set; } = Enumerable.Empty<Warranty>();
    public IEnumerable<AssigneeDM> Assignees { get; set; } = Enumerable.Empty<AssigneeDM>();
    public IEnumerable<FileDM> Files { get; set; } = Enumerable.Empty<FileDM>();
    public IEnumerable<FeedbackDetailsDM> Feedbacks { get; set; } = Enumerable.Empty<FeedbackDetailsDM>();
    public IEnumerable<Modification> Modifications { get; set; } = Enumerable.Empty<Modification>();
    public DateTimeOffset? SubmissionDeadline { get; set; }
    [JsonProperty("_etag")] public string ETag { get; set; }

    public static class Factory
    {
        public static DealSubmissionDM ToDataModel(DealSubmission submission) =>
            new DealSubmissionDM
            {
                Version = 1,
                Id = submission.Id,
                PartitionKey_DealId = submission.Id,
                Name = submission.Name,
                BrokerName = submission.BrokerName,
                BrokerCompanyId = submission.BrokerCompanyId,
                Terms = submission.Terms,
                Pricing = submission.Pricing,
                Enhancements = submission.Enhancements,
                Warranties = submission.Warranties,
                Assignees = submission.Assignees.Select(AssigneeDM.Factory.ToDataModel),
                Files = submission.Files.Select(FileDM.Factory.ToDataModel),
                Feedbacks = submission.Feedbacks.Select(FeedbackDetailsDM.Factory.ToDataModel),
                Modifications = submission.Modifications,
                SubmissionDeadline = submission.SubmissionDeadline,
                ETag = submission.ETag
            };

        public static DealSubmission ToEntity(DealSubmissionDM submission) =>
            new DealSubmission(
                submission.Id,
                submission.Name,
                submission.BrokerName,
                submission.BrokerCompanyId,
                submission.Terms,
                submission.Pricing,
                submission.Enhancements.ToImmutable(),
                submission.Warranties.ToImmutable(),
                submission.Assignees.Select(AssigneeDM.Factory.ToEntity).ToImmutable(),
                submission.Files.Select(FileDM.Factory.ToEntityForDeal).ToImmutable(),
                submission.Feedbacks.Select(FeedbackDetailsDM.Factory.ToEntity).ToImmutable(),
                submission.Modifications.ToImmutable(),
                submission.SubmissionDeadline
                ) { ETag = submission.ETag };
    }
}

internal class FeedbackDetailsDM
{
    public Guid FeedbackId { get; set; }
    public Guid InsuranceCompanyId { get; set; }
    public bool IsLive { get; set; }
    public IEnumerable<AssigneeDM> Assignees { get; set; } = Enumerable.Empty<AssigneeDM>();

    public static class Factory
    {
        public static FeedbackDetailsDM ToDataModel(FeedbackDetails feedbackDetails) =>
            new FeedbackDetailsDM
            {
                FeedbackId = feedbackDetails.FeedbackId,
                InsuranceCompanyId = feedbackDetails.InsuranceCompanyId,
                IsLive = feedbackDetails.IsLive,
                Assignees = feedbackDetails.Assignees.Select(AssigneeDM.Factory.ToDataModel)
            };

        public static FeedbackDetails ToEntity(FeedbackDetailsDM feedbackDetails) =>
            new FeedbackDetails(
                feedbackDetails.FeedbackId,
                feedbackDetails.InsuranceCompanyId,
                feedbackDetails.IsLive,
                feedbackDetails.Assignees.Select(AssigneeDM.Factory.ToEntity).ToImmutable()
                );
    }
}
