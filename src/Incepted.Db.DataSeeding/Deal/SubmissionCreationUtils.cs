using Incepted.Db.DataModels.DealDMs;
using Incepted.Db.DataModels.SharedDMs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using RandomNameGeneratorLibrary;

namespace Incepted.Db.DataSeeding.Deal;

internal static class SubmissionCreationUtils
{
    /// <summary>
    /// Creates a submission with no broker id or name, not submitted or modified-post-sumbission 
    /// (use Submitted() and Modified() methods for those)
    /// </summary>
    /// <returns>A populated submission</returns>
    public static DealSubmissionDM CreateSubmission()
    {
        Console.Write("Creating submission...");

        var placeGenerator = new PlaceNameGenerator();
        var name = $"Project {placeGenerator.GenerateRandomPlaceName()}";

        var submissionId = Guid.NewGuid();
        DealSubmissionDM submission = new DealSubmissionDM
        {
            Version = 1,
            Id = submissionId,
            PartitionKey_DealId = submissionId,
            Name = name,
            Terms =
            {
                InsuredAndBuyer = Guid.NewGuid().ToString(),
                InsuredAndBuyerJurisdiction = Jurisdiction.England,
                Target = Guid.NewGuid().ToString(),
                TargetJurisdiction = Jurisdiction.England,
                UBO = Guid.NewGuid().ToString(),
                UBOJurisdiction = Jurisdiction.England,
                Sellers = Guid.NewGuid().ToString(),
                Process = DealProcess.Some_deal_process,
                Industry = "Energy",
                TargetShortDescription = Guid.NewGuid().ToString(),
                FinancialInfo = Guid.NewGuid().ToString(),
                GeographicalFoorprint = Guid.NewGuid().ToString(),
                GoverningLaw = Jurisdiction.England,
                EmployeesNumber = new Random().Next(100, 1000),
                PurchasePriceMechanism = PurchasePriceMechanism.Locked_box_accounts,
                InsuredObligations = Guid.NewGuid().ToString(),
                PolicyDurationInMonthsForBusinessWarranties = 24,
                PolicyDurationInMonthsForFundamentalWarranties = 84,
                PolicyDurationInMonthsForTaxIdemnity = 84,
                BuySideAdvisors = new List<DealAdvisor>
                {
                    new DealAdvisor { Type = "Legal", Name = Guid.NewGuid().ToString() },
                    new DealAdvisor { Type = "Tax and Accounting", Name = Guid.NewGuid().ToString() },
                    new DealAdvisor { Type = "Commercial/Technical", Name = Guid.NewGuid().ToString() },
                    new DealAdvisor { Type = "Financial", Name = Guid.NewGuid().ToString() },
                },
                SellSideAdvisors = new List<DealAdvisor>
                {
                    new DealAdvisor { Type = "Legal", Name = Guid.NewGuid().ToString() },
                    new DealAdvisor { Type = "Tax and Accounting", Name = Guid.NewGuid().ToString() },
                    new DealAdvisor { Type = "Commercial/Technical", Name = Guid.NewGuid().ToString() },
                    new DealAdvisor { Type = "Financial", Name = Guid.NewGuid().ToString() },
                },
                BidDate = DateTime.Now.AddDays(60),
                SigningDate = DateTime.Now.AddDays(80),
                FinalPolicyDate = DateTime.Now.AddDays(120),
                Notes = "Lorem voluptua dolor et est eos lorem consequat et vero dolores in augue amet voluptua clita nonumy eirmod. Velit ipsum nihil takimata dolore erat erat tempor nostrud. Eirmod sit veniam aliquyam justo molestie vel. Et et vero kasd odio ipsum sanctus magna at sed dolore delenit consequat dolor. In ut diam dolor dignissim et diam gubergren justo voluptua. Labore iriure tempor stet te at sed. Et eos dolor. Minim takimata illum amet lorem vel aliquip stet est et amet diam erat."
            },
            Enhancements = RandomlySelectedEnhancements(),
            Warranties = Warranty.Factory.Default,
            Files = RandomFiles(name)
        };

        Console.WriteLine("DONE");

        return submission;
    }

    /// <summary>
    /// Adds broker details to the submission
    /// </summary>
    /// <param name="brokerCompanyId">The broker company id</param>
    /// <param name="brokerCompanyName">The broker name</param>
    /// <returns>A submission with broker details</returns>
    public static DealSubmissionDM WithBroker(this DealSubmissionDM submission, Guid brokerCompanyId, string brokerCompanyName)
    {
        Console.Write("Setting submission broker details...");

        submission.BrokerCompanyId = brokerCompanyId;
        submission.BrokerName = brokerCompanyName;

        Console.WriteLine("DONE");

        return submission;
    }

    /// <summary>
    /// Adds assignees
    /// </summary>
    /// <param name="submission">The submission to add assignees to</param>
    /// <param name="number">The number of assignees to add, defaults to 3</param>
    /// <returns>The submission with added assignees</returns>
    public static DealSubmissionDM WithRandomAssignees(this DealSubmissionDM submission, int number = 3)
    {
        Console.Write("Setting random submission assignees...");

        var personGenerator = new PersonNameGenerator();
        var assignees = new List<AssigneeDM>();

        for (int i = 0; i < number; i++)
        {
            var id = Guid.NewGuid();
            var firstName = personGenerator.GenerateRandomFirstName();
            var lastName = personGenerator.GenerateRandomLastName();

            assignees.Add(
                new AssigneeDM
                {
                    Version = 1,
                    Id = id,
                    UserId = $"auth0|{id.ToString().Replace("-", string.Empty)}",
                    Name = new HumanNameDM { First = firstName, Last = lastName },
                    Email = $"{firstName}.{lastName}@{submission.Name.Replace(" ", string.Empty)}.com".ToLower()
                });
        }

        submission.Assignees = submission.Assignees.ToList().Concat(assignees);

        Console.WriteLine("DONE");

        return submission;
    }

    /// <summary>
    /// Set the submission to submitted status
    /// </summary>
    /// <returns>A submitted submission</returns>
    public static DealSubmissionDM Submitted(this DealSubmissionDM submission, IEnumerable<FeedbackDetailsDM> feedbacks)
    {
        Console.Write("Setting submission to submitted status...");

        submission.Feedbacks = feedbacks.Select(f => new FeedbackDetailsDM
        {
            FeedbackId = f.FeedbackId,
            InsuranceCompanyId = f.InsuranceCompanyId,
            IsLive = false,
            Assignees = f.Assignees
        });

        Console.WriteLine("DONE");

        return submission;
    }

    /// <summary>
    /// Modify a submitted submission 
    /// </summary>
    /// <returns>A submitted submission with modifications</returns>
    public static DealSubmissionDM Submitted(this DealSubmissionDM submission, int numberOfModifications)
    {
        Console.Write("Setting modifications on a submitted submission...");

        var modifications = new List<Modification>();
        for (int i = 0; i < numberOfModifications; i++)
        {
            modifications.Add(new Modification(
                notes: "Lorem voluptua dolor et est eos lorem consequat et vero dolores in augue amet voluptua clita nonumy eirmod. Velit ipsum nihil takimata dolore erat erat tempor nostrud. Eirmod sit veniam aliquyam justo molestie vel. Et et vero kasd odio ipsum sanctus magna at sed dolore delenit consequat dolor. In ut diam dolor dignissim et diam gubergren justo voluptua. Labore iriure tempor stet te at sed. Et eos dolor. Minim takimata illum amet lorem vel aliquip stet est et amet diam erat.",
                timeStamp: DateTimeOffset.Now.AddDays(-i)
                ));
        }

        submission.Modifications = modifications;

        Console.WriteLine("DONE");

        return submission;
    }

    private static IEnumerable<Enhancement> RandomlySelectedEnhancements()
    {
        var random = new Random();
        var enhancements = new List<Enhancement>();
        foreach (var enhancement in Enhancement.Factory.Default)
        {
            enhancements.Add(enhancement.SetBrokerSelected(random.Next(2) == 1));
        }
        return enhancements;
    }
    private static IEnumerable<FileDM> RandomFiles(string submissionName)
    {
        return new List<FileDM>
        {
            new FileDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                FileName = $"SPA - {submissionName}.docx",
                StoredFileName = Path.GetRandomFileName(),
                FileType = FileType.SPA,
                LastModified = DateTimeOffset.UtcNow.AddDays(-3),
            },
            new FileDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                FileName = $"Warranties - {submissionName}.docx",
                StoredFileName = Path.GetRandomFileName(),
                FileType = FileType.Warranties,
                LastModified = DateTimeOffset.UtcNow.AddDays(-3),
            },
            new FileDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                FileName = $"IM - {submissionName}.docx",
                StoredFileName = Path.GetRandomFileName(),
                FileType = FileType.IM,
                LastModified = DateTimeOffset.UtcNow.AddDays(-3),
            },
            new FileDM
            {
                Version = 1,
                Id = Guid.NewGuid(),
                FileName = $"NDA - {submissionName}.docx",
                StoredFileName = Path.GetRandomFileName(),
                FileType = FileType.NDA,
                LastModified = DateTimeOffset.UtcNow.AddDays(-3),
            }
        };
    }
}
