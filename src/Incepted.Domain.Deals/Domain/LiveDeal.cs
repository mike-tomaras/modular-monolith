using Incepted.Shared.ValueTypes;
using System.Collections.Immutable;

namespace Incepted.Domain.Deals.Domain;

public class LiveDeal
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string BrokerName { get; private set; }
    public Guid BrokerCompanyId { get; private set; }
    public Guid SubmissionId { get; private set; }
    public string InsurerName { get; private set; }
    public Guid InsuranceCompanyId { get; private set; }
    public Guid FeedbackId { get; private set; }
    public IImmutableList<Assignee> AssigneesBroker { get; private set; }
    public IImmutableList<Assignee> AssigneesInsurer { get; private set; }
    public Money EnterpriseValue { get; private set; }
    

    public LiveDeal(
        Guid id,
        string name,
        string brokerName,
        Guid brokerCompanyId,
        Guid submissionId,
        string insurerName,
        Guid insuranceCompanyId,
        Guid feedbackId,
        IImmutableList<Assignee> assigneesBroker,
        IImmutableList<Assignee> assigneesInsurer,
        Money ev)
    {
        if (id == Guid.Empty) throw new ArgumentException("Deal Id can't be empty", $"{nameof(LiveDeal)} {nameof(id)}");
        if (brokerCompanyId == Guid.Empty) throw new ArgumentException("Broker Company Id can't be empty", $"{nameof(LiveDeal)} {nameof(brokerCompanyId)}");
        if (submissionId == Guid.Empty) throw new ArgumentException("Submission Id can't be empty", $"{nameof(LiveDeal)} {nameof(submissionId)}");
        if (insuranceCompanyId == Guid.Empty) throw new ArgumentException("Broker Company Id can't be empty", $"{nameof(LiveDeal)} {nameof(insuranceCompanyId)}");
        if (feedbackId == Guid.Empty) throw new ArgumentException("Feedback Id can't be empty", $"{nameof(LiveDeal)} {nameof(feedbackId)}");
        if (!IsNameValid(name)) throw new ArgumentException("Deal name can't be empty", $"{nameof(LiveDeal)} {nameof(name)}");
        if (!IsNameValid(brokerName)) throw new ArgumentException("Broker name can't be empty", $"{nameof(LiveDeal)} {nameof(brokerName)}");
        if (!IsNameValid(insurerName)) throw new ArgumentException("Broker name can't be empty", $"{nameof(LiveDeal)} {nameof(insurerName)}");

        Id = id;
        Name = name;
        BrokerName = brokerName;
        BrokerCompanyId = brokerCompanyId;
        SubmissionId = submissionId;
        InsurerName = insurerName;
        InsuranceCompanyId = insuranceCompanyId;
        FeedbackId = feedbackId;
        AssigneesBroker = assigneesBroker ?? ImmutableList.Create<Assignee>();
        AssigneesInsurer = assigneesInsurer ?? ImmutableList.Create<Assignee>();
        EnterpriseValue = ev;
    }

    private bool IsNameValid(string name) => !string.IsNullOrEmpty(name);
        
    public static class Factory
    {
        public static LiveDeal Create(DealSubmission submission, SubmissionFeedback feedback) =>
            new LiveDeal(
                id: Guid.NewGuid(),
                name: submission.Name,
                brokerName: submission.BrokerName,
                brokerCompanyId: submission.BrokerCompanyId,
                submissionId: submission.Id,
                insurerName: feedback.InsuranceCompanyName,
                insuranceCompanyId: feedback.InsuranceCompanyId,
                feedbackId: feedback.Id,
                assigneesBroker: submission.Assignees,
                assigneesInsurer: submission.Feedbacks.Single(i => i.FeedbackId == feedback.Id).Assignees,
                submission.Pricing.EnterpriseValue
                );
    }
}

public class EmptyLiveDeal : LiveDeal
{
    public EmptyLiveDeal()
        : base(Guid.NewGuid(), "Empty", "Empty", Guid.NewGuid(), Guid.NewGuid(), "Empty", Guid.NewGuid(), Guid.NewGuid(), ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Money())
    { }
}