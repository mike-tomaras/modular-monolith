using Incepted.Db.DataModels.DealDMs;
using Incepted.Db.DataModels.SharedDMs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using RandomNameGeneratorLibrary;

namespace Incepted.Db.DataSeeding.Deal;

internal static class FeedbackCreationUtils
{
    /// <summary>
    /// Creates a feedback with no insurer id or name, not submitted or declined
    /// (use Submitted() and Declined() methods for those)
    /// </summary>
    /// <returns>A populated feedback</returns>
    public static SubmissionFeedbackDM CreateFeedback(DealSubmissionDM submission)
    {
        Console.Write("Creating feedback...");

        var placeGenerator = new PlaceNameGenerator();
        var name = $"Project {placeGenerator.GenerateRandomPlaceName()}";

        var feedbackId = Guid.NewGuid();
        SubmissionFeedbackDM feedback = new SubmissionFeedbackDM
        {
            Version = 1,
            Id = feedbackId,
            PartitionKey_DealId = submission.Id,
            NdaAccepted = false,
            Submitted = false,
            Declined = false,
            IsLive = false,
            ForReview = false,
            Name = name,
            Notes = "Lorem voluptua dolor et est eos lorem consequat et vero dolores in augue amet voluptua clita nonumy eirmod. Velit ipsum nihil takimata dolore erat erat tempor nostrud. Eirmod sit veniam aliquyam justo molestie vel. Et et vero kasd odio ipsum sanctus magna at sed dolore delenit consequat dolor. In ut diam dolor dignissim et diam gubergren justo voluptua. Labore iriure tempor stet te at sed. Et eos dolor. Minim takimata illum amet lorem vel aliquip stet est et amet diam erat.",
            Pricing = FeedbackPricing.Factory.Parse(submission.Pricing),
            Enhancements = RandomlySelectedEnhancements(submission.Enhancements),
            Exclusions = RandomlySelectedExclusions(),
            ExcludedCountries = new List<string> { "Russia", "Palestine" },
            UwFocus = new List<string> { "Compliance", "Permits" },
            Warranties = RandomlySelectedWarranties(submission.Warranties)
        };

        Console.WriteLine("DONE");

        return feedback;
    }

    /// <summary>
    /// Adds insurer details to the feedback
    /// </summary>
    /// <param name="insurerCompanyId">The insurer company id</param>
    /// <param name="insurerCompanyName">The insurer name</param>
    /// <returns>A submission with broker details</returns>
    public static SubmissionFeedbackDM WithInsurer(this SubmissionFeedbackDM feedback, Guid insurerCompanyId, string insurerCompanyName)
    {
        Console.Write("Setting submission broker details...");

        feedback.InsuranceCompanyId = insurerCompanyId;
        feedback.InsuranceCompanyName = insurerCompanyName;

        Console.WriteLine("DONE");

        return feedback;
    }

    /// <summary>
    /// Set the feedback to submitted status
    /// </summary>
    /// <returns>A submitted feedback</returns>
    public static SubmissionFeedbackDM Submitted(this SubmissionFeedbackDM feedback)
    {
        Console.Write("Setting feedback to submitted status...");

        feedback.Submitted = true;
        feedback.Declined = false;

        Console.WriteLine("DONE");

        return feedback;
    }

    /// <summary>
    /// Set the feedback to declined status
    /// </summary>
    /// <returns>A Declined feedback</returns>
    public static SubmissionFeedbackDM Declined(this SubmissionFeedbackDM feedback)
    {
        Console.Write("Setting feedback to declined status...");

        feedback.Submitted = false;
        feedback.Declined = true;

        Console.WriteLine("DONE");

        return feedback;
    }

    private static IEnumerable<Enhancement> RandomlySelectedEnhancements(IEnumerable<Enhancement> enhancements)
    {
        var random = new Random();
        var newEnhancements = new List<Enhancement>();
        foreach (var enhancement in enhancements.Where(e => e.BrokerRequestsIt))
        {
            var newEnhancement = enhancement.SetInsurerSelected(random.Next(2) == 1);
            if (newEnhancement.InsurerOffersIt)
            {
                newEnhancement = newEnhancement.SetAP(random.Next(0, 10) / 100).ValueOr(newEnhancement);
                newEnhancement = newEnhancement.SetComment("Lorem voluptua dolor et est eos lorem consequat et vero dolores in augue amet voluptua clita nonumy eirmod. Velit ipsum nihil takimata dolore erat erat tempor nostrud.");
            }
            
            newEnhancements.Add(newEnhancement);
        }
        return newEnhancements;
    }
    private static IEnumerable<Exclusion> RandomlySelectedExclusions()
    {
        var random = new Random();
        var exclusions = new List<Exclusion>();
        foreach (var exclusion in Exclusion.Factory.Default)
        {
            var newExclusion = exclusion.SetInsurerSelected(random.Next(2) == 1);
            newExclusion = newExclusion.SetComment("Lorem voluptua dolor et est eos lorem consequat et vero dolores in augue amet voluptua clita nonumy eirmod. Velit ipsum nihil takimata dolore erat erat tempor nostrud.");
            
            exclusions.Add(newExclusion);
        }
        return exclusions;
    }
    private static IEnumerable<Warranty> RandomlySelectedWarranties(IEnumerable<Warranty> warranties)
    {
        var random = new Random();
        var newWarranties = new List<Warranty>();
        foreach (var warranty in warranties)
        {
            var newWarranty = warranty.SetCoveragePosition((CoveragePosition)random.Next(4)+1);
            newWarranty = warranty.SetKnowledgeScrape((KnowledgeScrape)random.Next(3)+1);
            if (newWarranty.CoveragePosition == CoveragePosition.Yes)
                newWarranty = newWarranty.SetComment("Lorem voluptua dolor et est eos lorem consequat et vero dolores in augue amet voluptua clita nonumy eirmod. Velit ipsum nihil takimata dolore erat erat tempor nostrud.");

            newWarranties.Add(newWarranty);
        }
        return newWarranties;
    }
}
