using Incepted.Db.DataModels.SharedDMs;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.ValueTypes;
using Newtonsoft.Json;

namespace Incepted.Db.DataModels.DealDMs;

internal class SubmissionFeedbackDM : BaseDM
{
    [JsonProperty("dealId")] public Guid PartitionKey_DealId { get; set; }//partition key
    public string Type => "feedback";
    public Guid InsuranceCompanyId { get; set; }
    public string InsuranceCompanyName { get; set; }
    public bool NdaAccepted { get; set; }
    public bool Submitted { get; set; }
    public bool Declined { get; set; }
    public bool IsLive { get; set; }
    public bool ForReview { get; set; }
    public string Name { get; set; }
    public string Notes { get; set; }
    public FeedbackPricing Pricing { get; set; }
    public IEnumerable<Enhancement> Enhancements { get; set; } = Enumerable.Empty<Enhancement>();
    public IEnumerable<Exclusion> Exclusions { get; set; } = Enumerable.Empty<Exclusion>();
    public IEnumerable<string> ExcludedCountries { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<string> UwFocus { get; set; } = Enumerable.Empty<string>();
    public IEnumerable<Warranty> Warranties { get; set; } = Enumerable.Empty<Warranty>();
    [JsonProperty("_etag")] public string ETag { get; set; }

    public SubmissionFeedbackDM Update(DealSubmission submission)
    {
        return new SubmissionFeedbackDM
        {
            Version = 1,
            Id = Id,
            PartitionKey_DealId = PartitionKey_DealId,
            InsuranceCompanyId = InsuranceCompanyId,
            InsuranceCompanyName = InsuranceCompanyName,
            NdaAccepted = NdaAccepted,
            Submitted = Submitted,
            Declined = Declined,
            IsLive = IsLive,
            ForReview = ForReview,
            Name = Name,
            Notes = Notes,
            Pricing = FeedbackPricing.Factory.Parse(submission.Pricing, existingPricing: Pricing),
            Enhancements = SubmissionFeedback.Factory.Parse(submission.Enhancements, Enhancements),
            Exclusions = Exclusions,
            ExcludedCountries = ExcludedCountries,
            UwFocus = UwFocus,
            Warranties = SubmissionFeedback.Factory.Parse(submission.Warranties, Warranties),
            ETag = submission.ETag
        };
    }
        
    public static class Factory
    {
        public static SubmissionFeedbackDM ToDataModel(SubmissionFeedback feedback) =>
            new SubmissionFeedbackDM
            {
                Version = 1,
                Id = feedback.Id,
                PartitionKey_DealId = feedback.SubmissionId,
                InsuranceCompanyId = feedback.InsuranceCompanyId,
                InsuranceCompanyName = feedback.InsuranceCompanyName,
                NdaAccepted = feedback.NdaAccepted,
                Submitted = feedback.Submitted,
                Declined = feedback.Declined,
                IsLive = feedback.IsLive,
                ForReview = feedback.ForReview,
                Name = feedback.Name,
                Notes = feedback.Notes,
                Pricing = feedback.Pricing,
                Enhancements = feedback.Enhancements,
                Exclusions = feedback.Exclusions,
                ExcludedCountries = feedback.ExcludedCountries,
                UwFocus = feedback.UwFocus,
                Warranties = feedback.Warranties,
                ETag = feedback.ETag
            };

        public static SubmissionFeedback ToEntity(SubmissionFeedbackDM feedback) =>
            new SubmissionFeedback(
                feedback.Id,
                feedback.PartitionKey_DealId,
                feedback.InsuranceCompanyId,
                feedback.InsuranceCompanyName,
                feedback.NdaAccepted,
                feedback.Submitted,
                feedback.Declined,
                feedback.IsLive,
                feedback.ForReview,
                feedback.Name,
                feedback.Notes,
                feedback.Pricing,
                feedback.Enhancements.ToImmutable(),
                feedback.Exclusions.ToImmutable(),
                feedback.ExcludedCountries.ToImmutable(),
                feedback.UwFocus.ToImmutable(),
                feedback.Warranties.ToImmutable()
                ) { ETag = feedback.ETag };
    }
}

