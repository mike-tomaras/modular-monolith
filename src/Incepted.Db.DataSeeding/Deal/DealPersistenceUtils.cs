using Incepted.Db.DataModels.CompanyDMs;
using Incepted.Db.DataModels.DealDMs;
using Microsoft.Azure.Cosmos;

namespace Incepted.Db.DataSeeding.Deal;

internal static class DealPersistenceUtils
{
    private static Database Database =>
        new CosmosClient(ConnStrings.LocalDevConnectionString).GetDatabase(id: "portaldb");
    private static Container ListContainer => Database.GetContainer("deals-list");
    private static Container Container => Database.GetContainer(id: "deals");

    public static async Task SaveAsync(this DealSubmissionDM submission, IEnumerable<SubmissionFeedbackDM>? feedbacks = null)
    {
        Console.Write("Saving submission...");

        if (submission.BrokerCompanyId == Guid.Empty || string.IsNullOrEmpty(submission.BrokerName))
        {
            Console.WriteLine($"ERROR: No broker details in submission with Id '{submission.Id}' and Name '{submission.Name}'. Use the 'WithBroker()' method to populate them.");
            return;
        }
        if (!submission.Assignees.Any())
        {
            Console.WriteLine($"ERROR: No assignees in submission with Id '{submission.Id}' and Name '{submission.Name}'. Use the 'WithRandomAssignees()' method to populate them.");
            return;
        }
        if (feedbacks == null) feedbacks = new List<SubmissionFeedbackDM>();
        if (submission.Feedbacks.Any() && !feedbacks.Any())
        {
            Console.WriteLine($"ERROR: The submission with Id '{submission.Id}' and Name '{submission.Name}' has feedback details but the SaveAsync method has an empty feedbacks argument. If the submission is submitted (i.e. has feedbacks) you need to pass the a list of feedbacks in the SaveAsync method to keep the database consistent.");
            return;
        }

        await Container.UpsertItemAsync(
            item: submission,
            partitionKey: new PartitionKey(submission.PartitionKey_DealId.ToString())
        );

        var listItem = submission.GetListItemForBroker();
        await ListContainer.UpsertItemAsync(
            item: listItem,
            partitionKey: new PartitionKey(listItem.PartitionKey_CompanyId.ToString())
        );

        foreach (var feedback in feedbacks)
        {
            await feedback.SaveAsync(submission);
        }

        Console.WriteLine("DONE");
    }
    public static async Task SaveAsync(this SubmissionFeedbackDM feedback, DealSubmissionDM submission)
    {
        Console.Write("Saving feedback...");
        if (feedback.InsuranceCompanyId == Guid.Empty || string.IsNullOrEmpty(feedback.InsuranceCompanyName))
        {
            Console.WriteLine($"ERROR: No insurer details in submission with Id '{feedback.Id}' and Name '{feedback.Name}'. Use the 'WithInsurer()' method to populate them.");
        }

        await Container.UpsertItemAsync(
            item: feedback,
            partitionKey: new PartitionKey(feedback.PartitionKey_DealId.ToString())
        );

        var listItem = submission.GetListItemForInsurer(feedback.InsuranceCompanyId);
        await ListContainer.UpsertItemAsync(
            item: listItem,
            partitionKey: new PartitionKey(listItem.PartitionKey_CompanyId.ToString())
        );

        Console.WriteLine("DONE");
    }

    private static SubmissionListItemDM GetListItemForBroker(this DealSubmissionDM submission)
    {
        var result = submission.GetListItem();
        result.PartitionKey_CompanyId = submission.BrokerCompanyId;
        return result;
    }
    private static SubmissionListItemDM GetListItemForInsurer(this DealSubmissionDM submission, Guid insurerId)
    {
        var result = submission.GetListItem();
        result.PartitionKey_CompanyId = insurerId;
        return result;
    }
    private static SubmissionListItemDM GetListItem(this DealSubmissionDM submission) =>
        new SubmissionListItemDM
        {
            Version = 1,
            Id = Guid.NewGuid(),
            SubmissionId = submission.Id,
            Name = submission.Name,
            BrokerName = submission.BrokerName,
            Industry = submission.Terms.Industry,
            SpaJurisdiction = submission.Terms.TargetJurisdiction.ToString(),
            IsSubmittedToInsurers = submission.Feedbacks.Any(),
            EnterpriseValue = submission.Pricing.EnterpriseValue,
            Assignees = submission.Assignees,
            SubmissionDeadline = submission.SubmissionDeadline
        };
}
