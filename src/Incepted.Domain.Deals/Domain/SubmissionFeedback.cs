using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Collections.Immutable;

namespace Incepted.Domain.Deals.Domain;

public class SubmissionFeedback
{
    public Guid Id { get; private set; }
    public Guid SubmissionId { get; private set; }
    public Guid InsuranceCompanyId { get; private set; }
    public string InsuranceCompanyName { get; private set; }
    public bool NdaAccepted { get; private set; }
    public bool Submitted { get; private set; }
    public bool Declined { get; private set; }
    public bool IsLive { get; private set; }
    public bool ForReview { get; private set; }
    public string Name { get; private set; }
    public string Notes { get; private set; }
    public FeedbackPricing Pricing { get; private set; }
    public IImmutableList<Enhancement> Enhancements { get; private set; }
    public IImmutableList<Exclusion> Exclusions { get; private set; }
    public IImmutableList<string> ExcludedCountries { get; private set; }
    public IImmutableList<string> UwFocus { get; private set; }
    public IImmutableList<Warranty> Warranties { get; private set; }
    public string ETag { get; set; }

    public SubmissionFeedback(
        Guid id,
        Guid submissionId,
        Guid insuranceCompanyId,
        string insuranceCompanyName,
        bool ndaAccepted,
        bool submitted,
        bool declined,
        bool isLive,
        bool forReview,
        string name,
        string notes,
        FeedbackPricing pricing,
        IImmutableList<Enhancement> enhancements,
        IImmutableList<Exclusion> exclusions,
        IImmutableList<string> excludedCountries,
        IImmutableList<string> uwFocus,
        IImmutableList<Warranty> warranties)
    {
        if (id == Guid.Empty) throw new ArgumentException("Feedback Id can't be empty", $"{nameof(SubmissionFeedback)} {nameof(id)}");
        if (submissionId == Guid.Empty) throw new ArgumentException("Feedback's Deal Id can't be empty", $"{nameof(SubmissionFeedback)} {nameof(submissionId)}");
        if (insuranceCompanyId == Guid.Empty) throw new ArgumentException("Insurance company Id can't be empty", $"{nameof(SubmissionFeedback)} {nameof(insuranceCompanyId)}");
        if (string.IsNullOrEmpty(insuranceCompanyName)) throw new ArgumentException("Insureance company name can't be empty", $"{nameof(SubmissionFeedback)} {nameof(insuranceCompanyName)}");
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Deal name can't be empty", $"{nameof(SubmissionFeedback)} {nameof(name)}");

        Id = id;
        SubmissionId = submissionId;
        InsuranceCompanyId = insuranceCompanyId;
        InsuranceCompanyName = insuranceCompanyName;
        NdaAccepted = ndaAccepted;
        Submitted = submitted;
        Declined = declined;
        IsLive = isLive;
        ForReview = forReview;
        Name = name;
        Notes = notes;
        Pricing = pricing ?? new FeedbackPricing();
        Enhancements = enhancements ?? ImmutableList.Create<Enhancement>();
        Exclusions = exclusions ?? ImmutableList.Create<Exclusion>();
        ExcludedCountries = excludedCountries ?? ImmutableList.Create<string>();
        UwFocus = uwFocus ?? ImmutableList.Create<string>();
        Warranties = warranties ?? ImmutableList.Create<Warranty>();
    }

    internal Option<SubmissionFeedback, ErrorCode> Update(SubmissionFeedbackDTO updatedDeal)
    {
        Notes = updatedDeal.Notes;
        Pricing = updatedDeal.Pricing;
        Enhancements = updatedDeal.Enhancements;
        Exclusions = updatedDeal.Exclusions;
        ExcludedCountries = updatedDeal.ExcludedCountries;
        UwFocus = updatedDeal.UwFocus;
        Warranties = updatedDeal.Warranties;
        ETag = updatedDeal.ETag;

        return this.Some<SubmissionFeedback, ErrorCode>();
    }

    internal Option<SubmissionFeedback, ErrorCode> AcceptNda()
    {
        NdaAccepted = true;

        return this.Some<SubmissionFeedback, ErrorCode>();
    }

    internal Option<SubmissionFeedback, ErrorCode> Submit()
    {
        Declined = false;
        Submitted = true;

        return this.Some<SubmissionFeedback, ErrorCode>();
    }

    internal Option<SubmissionFeedback, ErrorCode> Decline()
    {
        Submitted = false;
        Declined = true;

        return this.Some<SubmissionFeedback, ErrorCode>();
    }

    internal Option<SubmissionFeedback, ErrorCode> SubmissionModified()
    {
        if (!Submitted)
            return Option.None<SubmissionFeedback, ErrorCode>(DealErrorCodes.FailedToModifyFeedback_NotSubmittedYet);

        ForReview = true;

        return this.Some<SubmissionFeedback, ErrorCode>();
    }

    internal Option<SubmissionFeedback, ErrorCode> GoLive()
    {
        if (!Submitted)
            return Option.None<SubmissionFeedback, ErrorCode>(DealErrorCodes.FailedToGoLive_NotSubmittedYet);

        IsLive = true;

        return this.Some<SubmissionFeedback, ErrorCode>();
    }

    public static class Factory
    {
        public static SubmissionFeedbackDTO ToDTO(SubmissionFeedback deal)
        {
            return new SubmissionFeedbackDTO(
                                deal.Id,
                                deal.SubmissionId,
                                deal.InsuranceCompanyId,
                                deal.InsuranceCompanyName,
                                deal.NdaAccepted,
                                deal.Submitted,
                                deal.Declined,
                                deal.IsLive,
                                deal.ForReview,
                                deal.Name,
                                deal.Notes,
                                deal.Pricing,
                                deal.Enhancements,
                                deal.Exclusions,
                                deal.ExcludedCountries,
                                deal.UwFocus,
                                deal.Warranties,
                                deal.ETag
                                );
        }

        public static SubmissionFeedback Create(Guid insuranceCompanyId, string insuranceCompanyName, DealSubmission submission)
        {
            if (insuranceCompanyId == Guid.Empty) throw new ArgumentException("Insurance company Id can't be empty", $"{nameof(SubmissionFeedback)} {nameof(insuranceCompanyId)}");

            return new SubmissionFeedback(
                id: Guid.NewGuid(),
                submissionId: submission.Id,
                insuranceCompanyId,
                insuranceCompanyName,
                ndaAccepted: false,
                submitted: false,
                declined: false,
                isLive: false,
                forReview: false,
                submission.Name,
                notes: string.Empty,
                FeedbackPricing.Factory.Parse(submission.Pricing),
                Parse(submission.Enhancements).ToImmutable(),
                Exclusion.Factory.Default,
                ImmutableList.Create<string>(),
                ImmutableList.Create<string>(),
                Parse(submission.Warranties).ToImmutable()
                );
        }

        public static IEnumerable<Enhancement> Parse(IEnumerable<Enhancement> enhancements, IEnumerable<Enhancement> existingEnhancements = default)
        {
            if (existingEnhancements == null) existingEnhancements= new List<Enhancement>();
            var result = enhancements.Where(e => e.BrokerRequestsIt).ToList();            

            foreach (var enhancement in existingEnhancements)
            {
                Func<Enhancement, bool> predicate = e =>
                        e.Title == enhancement.Title &&
                        e.Description == enhancement.Description;

                var existingEnhancement = 
                    result
                    .SingleOrDefault(predicate);

                if (existingEnhancement != null) result.ReplaceInList(predicate, existingEnhancement);
            }

            return result;
        }

        public static IEnumerable<Warranty> Parse(IEnumerable<Warranty> warranties, IEnumerable<Warranty> existingWarranties = default)
        {
            if (existingWarranties == null) existingWarranties = new List<Warranty>();
            var result = warranties.ToList();

            foreach (var warranty in existingWarranties)
            {
                Func<Warranty, bool> predicate = w =>
                        w.Order == warranty.Order &&
                        w.Description == warranty.Description;

                var existingWarranty =
                    result
                    .SingleOrDefault(predicate);

                if (existingWarranty != null) result.ReplaceInList(predicate, existingWarranty);
            }

            return result;
        }
    }
}

public class EmptyFeedback : SubmissionFeedback
{
    public EmptyFeedback()
        : base(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Empty", false, false, false, false, false, "Empty", string.Empty, new FeedbackPricing(), Enhancement.Factory.Default, Exclusion.Factory.Default, ImmutableList.Create<string>(), ImmutableList.Create<string>(), ImmutableList.Create<Warranty>())
    { }
}
