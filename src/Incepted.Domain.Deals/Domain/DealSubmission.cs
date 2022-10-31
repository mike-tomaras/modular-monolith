using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using Optional;
using Serilog;
using System.Collections.Immutable;

namespace Incepted.Domain.Deals.Domain;

public class DealSubmission
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string BrokerName { get; private set; }
    public Guid BrokerCompanyId { get; private set; }
    public BasicTerms Terms { get; private set; }
    public SubmissionPricing Pricing { get; private set; }
    public IImmutableList<Enhancement> Enhancements { get; private set; }
    public IImmutableList<Warranty> Warranties { get; private set; }
    public IImmutableList<Assignee> Assignees { get; private set; }
    public IImmutableList<DealFile> Files { get; private set; }
    public IImmutableList<FeedbackDetails> Feedbacks { get; private set; }
    public IImmutableList<Modification> Modifications { get; private set; }
    public DateTimeOffset? SubmissionDeadline { get; private set; }
    public string ETag { get; set; }

    public DealSubmission(
        Guid id,
        string name,
        string brokerName,
        Guid brokerCompanyId,
        BasicTerms terms,
        SubmissionPricing pricing,
        IImmutableList<Enhancement> enhancements,
        IImmutableList<Warranty> warranties,
        IImmutableList<Assignee> assignees,
        IImmutableList<DealFile> files,
        IImmutableList<FeedbackDetails> insurersSubmittedTo,
        IImmutableList<Modification> modifications,
        DateTimeOffset? submissionDeadline)
    {
        if (id == Guid.Empty) throw new ArgumentException("Deal Id can't be empty", $"{nameof(DealSubmission)} {nameof(id)}");
        if (brokerCompanyId == Guid.Empty) throw new ArgumentException("Broker Company Id can't be empty", $"{nameof(DealSubmission)} {nameof(brokerCompanyId)}");
        if (!IsNameValid(name)) throw new ArgumentException("Deal name can't be empty", $"{nameof(DealSubmission)} {nameof(name)}");
        if (!IsNameValid(brokerName)) throw new ArgumentException("Broker name can't be empty", $"{nameof(DealSubmission)} {nameof(brokerName)}");

        Id = id;
        Name = name;
        BrokerName = brokerName;
        BrokerCompanyId = brokerCompanyId;
        Terms = terms ?? new BasicTerms();
        Pricing = pricing ?? new SubmissionPricing();
        Enhancements = enhancements;
        Warranties = warranties;
        Assignees = assignees ?? ImmutableList.Create<Assignee>();
        Files = files?.OrderByDescending(f => f.LastModified).ToImmutable() ?? ImmutableList.Create<DealFile>();
        Feedbacks = insurersSubmittedTo ?? ImmutableList.Create<FeedbackDetails>();
        Modifications = modifications ?? ImmutableList.Create<Modification>();
        SubmissionDeadline = submissionDeadline;//no validation as a deadline may have passed and it needs extending        
    }

    private bool IsNameValid(string name) => !string.IsNullOrEmpty(name);
    private bool IsDeadlineValid(DateTimeOffset? deadline) => deadline == null ? true : deadline > DateTimeOffset.Now;

    internal Option<DealSubmission, ErrorCode> Update(DealSubmissionDTO updatedDeal)
    {
        if (!IsDeadlineValid(updatedDeal.SubmissionDeadline))
        {
            Log.Warning("Attempted to edit deal submission deadline with Id {DealId} with a date in the past", Id);
            return Option.None<DealSubmission, ErrorCode>(DealErrorCodes.FailedToUpdateDeal_InvalidDeadline);
        }

        Terms = updatedDeal.Terms;
        Pricing = updatedDeal.Pricing;
        Enhancements = updatedDeal.Enhancements;
        var updatedFiles = updatedDeal.Files.Select(DealFile.Factory.ToEntity);
        Files = updatedFiles.ToImmutable();
        Warranties = updatedDeal.Warranties;
        SubmissionDeadline = updatedDeal.SubmissionDeadline;
        ETag = updatedDeal.ETag;

        return this.Some<DealSubmission, ErrorCode>();
    }

    internal Option<DealSubmission, ErrorCode> UpdateAssignees(IImmutableList<Assignee> newAssignees, Guid companyId)
    {
        if (companyId == BrokerCompanyId)
        {
            Assignees = newAssignees.ToImmutable();

            return this.Some<DealSubmission, ErrorCode>();
        }

        return
            Feedbacks.SomeWhen(list => list != null && list.Any(), DealErrorCodes.AssigneesCompanyDoesNotExistInSubmission)
            .FlatMap(list => list
                            .SingleOrDefault(f => f.InsuranceCompanyId == companyId)
                            .SomeNotNull(DealErrorCodes.AssigneesCompanyDoesNotExistInSubmission)
                )
            .FlatMap(feedback => feedback.UpdateAssignees(newAssignees))
            .Map(newFeedback => Feedbacks = Utils.Replace(Feedbacks, f => f.InsuranceCompanyId == companyId, newFeedback))
            .Map(_ => this);
    }

    internal Option<DealSubmission, ErrorCode> AddFiles(IImmutableList<DealFile> newFiles)
    {
        Files = Files.AddRange(newFiles);

        return this.Some<DealSubmission, ErrorCode>();
    }
    internal Option<DealSubmission, ErrorCode> RemoveFile(Guid fileId)
    {
        Files = Files.Remove(f => f.Id == fileId);

        return this.Some<DealSubmission, ErrorCode>();
    }

    internal Option<DealSubmission, ErrorCode> Submit(IImmutableList<FeedbackDetails> insurers, DateTimeOffset submissionDeadline)
    {
        if (!IsDeadlineValid(submissionDeadline))
            return Option.None<DealSubmission, ErrorCode>(DealErrorCodes.FailedToUpdateDeal_InvalidDeadline);

        SubmissionDeadline = submissionDeadline;
        Feedbacks = insurers;

        return this.Some<DealSubmission, ErrorCode>();
    }

    internal Option<DealSubmission, ErrorCode> ModifySubmission(string notes)
    {
        if (string.IsNullOrEmpty(notes))
            return Option.None<DealSubmission, ErrorCode>(DealErrorCodes.FailedToModifyDeal_NoNotes);
        if (!Feedbacks.Any())
            return Option.None<DealSubmission, ErrorCode>(DealErrorCodes.FailedToModifyDeal_NotSubmittedYet);

        Modifications = Modifications.Add(new Modification(notes, DateTimeOffset.UtcNow));

        return this.Some<DealSubmission, ErrorCode>();
    }

    internal Option<DealSubmission, ErrorCode> GoLive(Guid feedbackId)
    {
        if (!Feedbacks.Any())
            return Option.None<DealSubmission, ErrorCode>(DealErrorCodes.FailedToGoLive_NotSubmittedYet);

        return
        Feedbacks
            .SingleOrDefault(i => i.FeedbackId == feedbackId)
            .SomeNotNull(DealErrorCodes.FeedbackNotFound)
            .FlatMap(feedbackDetail => feedbackDetail.GoLive())
            .Map(feedbackDetail => Feedbacks = Feedbacks.Replace(i => i.FeedbackId == feedbackId, feedbackDetail))
            .FlatMap(_ => this.Some<DealSubmission, ErrorCode>());        
    }

    public static class Factory
    {
        public static DealListItemDTO ToListItemDTO(DealSubmission deal)
        {
            return new DealListItemDTO(
                                    deal.Id,
                                    deal.Name,
                                    deal.BrokerName,
                                    deal.Terms.Industry,
                                    deal.Terms.TargetJurisdiction.ToString(),
                                    deal.Feedbacks.Any(),
                                    deal.Pricing.EnterpriseValue,
                                    deal.Assignees.Select(a => new EmployeeDTO(a.Id, a.UserId.ToString(), a.Name.First, a.Name.Last, a.Email.Address)).ToImmutable(),
                                    deal.SubmissionDeadline
                                    );
        }

        public static DealSubmissionDTO ToDTO(DealSubmission deal)
        {
            return new DealSubmissionDTO(
                                deal.Id,
                                deal.Name,
                                deal.BrokerName,
                                deal.Terms,
                                deal.Pricing,
                                deal.Enhancements,
                                deal.Warranties,
                                deal.Assignees.Select(a => new EmployeeDTO(a.Id, a.UserId.ToString(), a.Name.First, a.Name.Last, a.Email.Address)).ToImmutable(),
                                deal.Files.Select(DealFile.Factory.ToDTO).ToImmutable(),
                                deal.Feedbacks.Select(i => (i.InsuranceCompanyId, i.FeedbackId)).ToImmutable(),
                                deal.Modifications,
                                deal.SubmissionDeadline,
                                deal.ETag
                                );
        }
    }
}

public class EmptySubmission : DealSubmission
{
    public EmptySubmission()
        : base(Guid.NewGuid(), "Empty", "Empty", Guid.NewGuid(), new BasicTerms(), new SubmissionPricing(), Enhancement.Factory.Default, ImmutableList.Create<Warranty>(), new List<Assignee> { Assignee.Factory.Empty }.ToImmutable(), ImmutableList.Create<DealFile>(), ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.MaxValue)
    { }
}

public class FeedbackDetails
{
    public Guid FeedbackId { get; init; }
    public Guid InsuranceCompanyId { get; init; }
    public bool IsLive { get; init; }
    public IImmutableList<Assignee> Assignees { get; private set; }


    public FeedbackDetails(Guid feedbackId, Guid insuranceCompanyId, bool isLive, IImmutableList<Assignee> assignees)
    {
        if (feedbackId == Guid.Empty) throw new ArgumentException("Feedback detail feedback Id can't be empty", $"{nameof(FeedbackDetails) } {nameof(feedbackId)}");
        if (insuranceCompanyId == Guid.Empty) throw new ArgumentException("Feedback detail insurer Id can't be empty", $"{nameof(FeedbackDetails)} {nameof(insuranceCompanyId)}");

        FeedbackId = feedbackId;
        InsuranceCompanyId = insuranceCompanyId;
        IsLive = isLive;
        Assignees = assignees ?? ImmutableList.Create<Assignee>();
    }

    internal Option<FeedbackDetails, ErrorCode> UpdateAssignees(IImmutableList<Assignee> newAssignees)
    {
        Assignees = newAssignees;

        return this.Some<FeedbackDetails, ErrorCode>();
    }

    internal Option<FeedbackDetails, ErrorCode> GoLive() => 
        new FeedbackDetails(FeedbackId, InsuranceCompanyId, true, Assignees).Some<FeedbackDetails, ErrorCode>();

    public static class Factory
    {
        public static FeedbackDetails Create(Guid feedbackId, Guid insuranceCompanyId) => 
            new FeedbackDetails(feedbackId, insuranceCompanyId, false, ImmutableList.Create<Assignee>());
    }
}


