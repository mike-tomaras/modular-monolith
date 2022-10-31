using Incepted.Domain.Deals.Application;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using Optional;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

namespace Incepted.Db.Repos;

internal class DevDealRepo : IDealRepo
{
    private static readonly DealSeedData _db = new DealSeedData();

    public Task<Option<Unit, ErrorCode>> Create(SubmissionFeedback newFeedback, DealSubmission submission)
    {
        _db.Feedbacks.Add(newFeedback);
        return Task.FromResult(Option.Some<Unit, ErrorCode>(new Unit()));
    }

    public Task<Option<Unit, ErrorCode>> Create(DealSubmission newDeal)
    {
        _db.Submissions.Add(newDeal);
        return Task.FromResult(Option.Some<Unit, ErrorCode>(new Unit()));
    }

    public Task<Option<Unit, ErrorCode>> Create(LiveDeal newDeal)
    {
        _db.LiveDeals.Add(newDeal);
        return Task.FromResult(Option.Some<Unit, ErrorCode>(new Unit()));
    }

    public Task<Option<IImmutableList<SubmissionFeedback>, ErrorCode>> GetAllFeedbackDetails(Guid submissionId)
    {
        return Task.FromResult(_db.Feedbacks
            .Where(feedback => feedback.SubmissionId == submissionId)
            .ToImmutable()
            .NoneWhen(l => !l.Any(), DealErrorCodes.FeedbackNotFound));
    }

    public Task<Option<SubmissionFeedback, ErrorCode>> GetFeedbackDetails(Guid feedbackId, Guid submissionId)
    {
        //TODO: implement db
        return Task.FromResult(_db.Feedbacks
            .SingleOrDefault(feedback => feedback.SubmissionId == submissionId)
            .SomeNotNull(DealErrorCodes.DealNotFound));
    }

    public Task<Option<DealSubmission, ErrorCode>> GetSubmissionDetails(Guid submissionId)
    {
        //TODO: implement db
        return Task.FromResult(_db.Submissions
            .SingleOrDefault(deal => deal.Id == submissionId)
            .SomeNotNull(DealErrorCodes.DealNotFound));
    }

    public Task<Option<IImmutableList<DealListItemDTO>, ErrorCode>> GetSubmissions(Guid companyId, CompanyType type)
    {
        //TODO: implement db
        return Task.FromResult(_db.Submissions
            .NoneWhen(list => list.Count() == 0, DealErrorCodes.NoDealsFound)
            .Map(l => l.Select(DealSubmission.Factory.ToListItemDTO).ToImmutable()));
    }

    public Task<Option<Unit, ErrorCode>> Modify(DealSubmission deal)
    {
        //TODO: implement db but only save modifications
        var dealToReplace = _db.Submissions.Single(d => d.Id == deal.Id);
        var index = _db.Submissions.IndexOf(dealToReplace);
        _db.Submissions[index] = deal;

        return Task.FromResult(Option.Some<Unit, ErrorCode>(new Unit()));
    }

    public Task<Option<Unit, ErrorCode>> Update(DealSubmission deal)
    {
        //TODO: implement db but do not save modifiations
        var dealToReplace = _db.Submissions.Single(d => d.Id == deal.Id);
        var index = _db.Submissions.IndexOf(dealToReplace);
        _db.Submissions[index] = deal;

        return Task.FromResult(Option.Some<Unit, ErrorCode>(new Unit()));
    }

    public Task<Option<Unit, ErrorCode>> Update(SubmissionFeedback feedback)
    {
        //TODO: implement db
        var feedbackToReplace = _db.Feedbacks.Single(f => f.Id == feedback.Id);
        var index = _db.Feedbacks.IndexOf(feedbackToReplace);
        _db.Feedbacks[index] = feedback;

        return Task.FromResult(Option.Some<Unit, ErrorCode>(new Unit()));
    }
}

[ExcludeFromCodeCoverage]
public class DealSeedData
{
    public List<DealSubmission> Submissions { get; set; }
    public List<SubmissionFeedback> Feedbacks { get; set; } = new List<SubmissionFeedback>();
    public List<LiveDeal> LiveDeals { get; set; } = new List<LiveDeal>();

    public DealSeedData()
    {
        var brokerCompanyId = Guid.Parse("c1955284-c461-4a39-9a06-a31b18b23a33");
        var insurerCompanyId1 = Guid.Parse("b065f679-d19f-4c3f-a948-a29377e91982");
        var insurerCompanyId2 = Guid.Parse("ad4546dc-7386-4300-a0de-f202cf14b8cf");
        var insurerCompanyId3 = Guid.Parse("add7bcc8-6efc-457a-901f-cab7d5730c1e");
        var insurerCompanyId4 = Guid.Parse("b3f338b0-4cca-481d-a603-e8295bd8e35d");
        var insurerCompanyId5 = Guid.Parse("3e470d73-6708-4360-80cd-4e22e957100b");
        var feedbackIdsForInsurers = new List<(Guid insurerId, Guid submissionId, Guid feedbackId)>
        {
            (insurerCompanyId1, Guid.Parse("2fdf39bc-3558-4450-8886-6eae180b2ad7"), Guid.Parse("d98ac73a-371a-412c-8eaf-7483dec344fe")),
            (insurerCompanyId1, Guid.Parse("a4499460-2f4f-47a2-ad50-841fb91ab343"), Guid.Parse("b21ae7d8-38c5-4da0-b068-b1f040860c62")),
            (insurerCompanyId1, Guid.Parse("f43c2203-a191-4485-94d9-a95ac91fb90b"), Guid.Parse("b9fd6174-a09c-45b0-9a56-a96de1e92c32")),
            (insurerCompanyId1, Guid.Parse("abac8c37-3e26-4bca-a51c-a9fc66de11a0"), Guid.Parse("ccbb67c5-83b5-4dee-949f-f7333f0cd0fd")),
            (insurerCompanyId1, Guid.Parse("b3c4faa2-6511-4985-b849-5fb590c51e5f"), Guid.Parse("21bf0569-852b-481b-97b7-a36736a042f5")),
            (insurerCompanyId2, Guid.Parse("2fdf39bc-3558-4450-8886-6eae180b2ad7"), Guid.Parse("2dcbcb9f-5f1a-4f35-9a30-cd2424bacfbd")),
            (insurerCompanyId2, Guid.Parse("a4499460-2f4f-47a2-ad50-841fb91ab343"), Guid.Parse("d6de829f-2d0c-48aa-82be-d2da5b599d70")),
            (insurerCompanyId2, Guid.Parse("f43c2203-a191-4485-94d9-a95ac91fb90b"), Guid.Parse("709c13ca-e0e3-416e-a2a7-d14df32ccda0")),
            (insurerCompanyId2, Guid.Parse("abac8c37-3e26-4bca-a51c-a9fc66de11a0"), Guid.Parse("48fb16a0-3630-4156-ab9e-f3ecef458e6a")),
            (insurerCompanyId2, Guid.Parse("b3c4faa2-6511-4985-b849-5fb590c51e5f"), Guid.Parse("4d112f1c-57b2-43ee-a458-3e047e3db190")),
            (insurerCompanyId3, Guid.Parse("2fdf39bc-3558-4450-8886-6eae180b2ad7"), Guid.Parse("633cc1b3-b168-4c70-8666-2ab62d09393c")),
            (insurerCompanyId3, Guid.Parse("a4499460-2f4f-47a2-ad50-841fb91ab343"), Guid.Parse("c5a99ce7-8930-401d-9977-ed29265162f4")),
            (insurerCompanyId3, Guid.Parse("f43c2203-a191-4485-94d9-a95ac91fb90b"), Guid.Parse("f5157eac-a111-4eba-a2c9-9d3d9be23c19")),
            (insurerCompanyId3, Guid.Parse("abac8c37-3e26-4bca-a51c-a9fc66de11a0"), Guid.Parse("4a276222-c0ed-4167-acb9-52bcf38608f3")),
            (insurerCompanyId3, Guid.Parse("b3c4faa2-6511-4985-b849-5fb590c51e5f"), Guid.Parse("05d13ca4-1c34-400c-bb31-366940aa3fd4")),
            (insurerCompanyId4, Guid.Parse("2fdf39bc-3558-4450-8886-6eae180b2ad7"), Guid.Parse("9f2f5c4f-5a5f-42bb-b6be-2446cac3bef4")),
            (insurerCompanyId4, Guid.Parse("a4499460-2f4f-47a2-ad50-841fb91ab343"), Guid.Parse("aa5126a0-5b34-479a-b547-b3344568730d")),
            (insurerCompanyId4, Guid.Parse("f43c2203-a191-4485-94d9-a95ac91fb90b"), Guid.Parse("fcf93603-9f06-41a8-bb5a-dfddf4aa4cb1")),
            (insurerCompanyId4, Guid.Parse("abac8c37-3e26-4bca-a51c-a9fc66de11a0"), Guid.Parse("9f7ad6e1-5a18-451d-9e37-d97b47021e13")),
            (insurerCompanyId4, Guid.Parse("b3c4faa2-6511-4985-b849-5fb590c51e5f"), Guid.Parse("fc433a04-f48d-4525-b1a1-e3676e36b100")),
            (insurerCompanyId5, Guid.Parse("2fdf39bc-3558-4450-8886-6eae180b2ad7"), Guid.Parse("c339d8db-0c6f-43e5-802b-cea995d1eba2")),
            (insurerCompanyId5, Guid.Parse("a4499460-2f4f-47a2-ad50-841fb91ab343"), Guid.Parse("8dc705d4-1440-4747-b3bf-e51e77574635")),
            (insurerCompanyId5, Guid.Parse("f43c2203-a191-4485-94d9-a95ac91fb90b"), Guid.Parse("9502d587-7ac9-4f99-8e81-3690c93ac23f")),
            (insurerCompanyId5, Guid.Parse("abac8c37-3e26-4bca-a51c-a9fc66de11a0"), Guid.Parse("2b171636-1163-4aa5-bba8-5052e322349f")),
            (insurerCompanyId5, Guid.Parse("b3c4faa2-6511-4985-b849-5fb590c51e5f"), Guid.Parse("7a52e427-193b-4d75-9c45-4f69ad81eeb0"))
        };
        
        SeedDeals(brokerCompanyId, feedbackIdsForInsurers);
        SeedFeedbacks(insurerCompanyId1, feedbackIdsForInsurers, submitted: true, declined: false);
        SeedFeedbacks(insurerCompanyId2, feedbackIdsForInsurers, submitted: true, declined: false);
        SeedFeedbacks(insurerCompanyId3, feedbackIdsForInsurers, submitted: true, declined: false);
        SeedFeedbacks(insurerCompanyId4, feedbackIdsForInsurers, submitted: false, declined: false);
        SeedFeedbacks(insurerCompanyId5, feedbackIdsForInsurers, submitted: false, declined: true);
    }

    private void SeedDeals(Guid brokerCompanyId, List<(Guid insurerId, Guid submissionId, Guid feedbackId)> feedbackIds)
    {
        var terms = new BasicTerms
        {
            InsuredAndBuyer = "This company",
            InsuredAndBuyerJurisdiction = Jurisdiction.England,
            Target = "That company",
            TargetJurisdiction = Jurisdiction.England,
            UBO = "That other company",
            UBOJurisdiction = Jurisdiction.England,
            Sellers = "This one",
            Process = DealProcess.Some_deal_process,
            Industry = "Renewables",
            TargetShortDescription = string.Empty,
            FinancialInfo = string.Empty,
            GeographicalFoorprint = string.Empty,
            GoverningLaw = Jurisdiction.England,
            EmployeesNumber = 100,
            PurchasePriceMechanism = PurchasePriceMechanism.Locked_box_accounts,
            InsuredObligations = string.Empty,
            PolicyDurationInMonthsForBusinessWarranties = 24,
            PolicyDurationInMonthsForFundamentalWarranties = 84,
            PolicyDurationInMonthsForTaxIdemnity = 84,
            BuySideAdvisors = new List<DealAdvisor>
            {
                new DealAdvisor { Type = "Legal", Name = "asd" },
                new DealAdvisor { Type = "Tax and Accounting", Name = "asd" },
                new DealAdvisor { Type = "Commercial/Technical" },
                new DealAdvisor { Type = "Financial" },
            },
            SellSideAdvisors = new List<DealAdvisor>
            {
                new DealAdvisor { Type = "Legal" },
                new DealAdvisor { Type = "Tax and Accounting" },
                new DealAdvisor { Type = "Commercial/Technical" , Name = "asd"},
                new DealAdvisor { Type = "Financial" , Name = "asd"},
            },
            BidDate = DateTime.Now.AddDays(10),
            SigningDate = DateTime.Now.AddDays(10),
            FinalPolicyDate = DateTime.Now.AddDays(10),
            Notes = "Sea consequat dolor erat sed sed at voluptua rebum diam congue. Duo sit rebum consetetur est. Dolor sit dolore tempor sed duo et. Tempor lorem sit ut lorem in tempor diam.\n" +
                            "Justo et et eirmod duo elit praesent sed dolor sit sanctus.Lorem consetetur feugiat elitr eleifend hendrerit consequat tempor autem diam rebum ut sanctus et eos et.Takimata dolor elitr consectetuer esse facer nonumy luptatum sea.Labore lobortis et tempor odio dolores ut praesent no eirmod clita sed dolor feugait takimata aliquyam.Ipsum duis commodo sit veniam ea et nonumy aliquam.Aliquip ad eos hendrerit consetetur elitr quod ipsum praesent diam feugait tempor dolores et nonumy ipsum erat erat.Sit ad diam nulla stet.Dolores takimata eirmod facilisi ut.Luptatum et tempor et.Lorem praesent duo lorem.Takimata eos ipsum dolore no vel nonummy sanctus stet diam takimata ut elitr et eos eos gubergren commodo.Justo ullamcorper duis volutpat sanctus sed consetetur aliquyam tempor labore sadipscing kasd.Dolores iriure quis sanctus lorem magna diam lobortis kasd magna at nonumy hendrerit consetetur sed est gubergren." +
                            "Ea feugait congue.Eu sanctus te est illum ut in ex et erat.Liber dolor est."
        };

        var dev_assignees = new List<Assignee>
        {
            new Assignee(Guid.NewGuid(), new UserId("auth0|62818ca9b0997000699020da"), new HumanName("Mike", "TheInsurer"), new MailAddress("email@email.com")),
            new Assignee(Guid.NewGuid(), new UserId("auth0|626e80056c48dc006a2de396"), new HumanName("Konrad", "Rotthege"), new MailAddress("email@email.com")),
            new Assignee(Guid.NewGuid(), new UserId("auth0|626e80401d742f006f2ab6a6"), new HumanName("Jamie", "Brown"), new MailAddress("email@email.com")),
            new Assignee(Guid.NewGuid(), new UserId("auth0|62492cd27810d5006999aa22"), new HumanName("Mike", "Tomaras"), new MailAddress("email@email.com")),
            new Assignee(Guid.NewGuid(), new UserId("auth0|6255a0d5c0f77100691fba5c"), new HumanName("John", "Doe"), new MailAddress("email@email.com")),
            new Assignee(Guid.NewGuid(), new UserId("auth0|62c9565049c4f87c02fce444"), new HumanName("MikeProd", "TheInsurer"), new MailAddress("mike@insurer.com")),
            new Assignee(Guid.NewGuid(), new UserId("auth0|62c95739d3dc4f88c18c0b18"), new HumanName("Konrad", "Rotthege"), new MailAddress("konrad@broker.com")),
            new Assignee(Guid.NewGuid(), new UserId("auth0|62c956f07b12303583eef4ab"), new HumanName("Jamie", "Brown"), new MailAddress("jamie@broker.com")),
        };



        var historicalFilesA = new List<DealFile>
        {
            new DealFile(Guid.NewGuid(), "previous document A.docx", "acb.def", FileType.SPA, DateTimeOffset.Now.AddDays(-11)),
            new DealFile(Guid.NewGuid(), "previous document B.xlsx", "ghi.jkl", FileType.IM, DateTimeOffset.Now.AddDays(-17)),
            new DealFile(Guid.NewGuid(), "previous document C.pdf", "mno.pqr", FileType.NDA, DateTimeOffset.Now.AddMonths(-2).AddDays(-13)),
        };
        var historicalFilesB = new List<DealFile>
        {
            new DealFile(Guid.NewGuid(), "previous document A.docx", "acb.def", FileType.SPA, DateTimeOffset.Now.AddDays(-21)),
            new DealFile(Guid.NewGuid(), "previous document B.xlsx", "ghi.jkl", FileType.IM, DateTimeOffset.Now.AddDays(-27)),
            new DealFile(Guid.NewGuid(), "previous document C.pdf", "mno.pqr", FileType.NDA, DateTimeOffset.Now.AddMonths(-3).AddDays(-23)),
        };
        var files = new List<DealFile>
        {
            new DealFile(Guid.NewGuid(), "document A.docx", "acb.def", FileType.SPA, DateTimeOffset.Now.AddDays(-1)),
            new DealFile(Guid.NewGuid(), "document B.xlsx", "ghi.jkl", FileType.IM, DateTimeOffset.Now.AddDays(-7)),
            new DealFile(Guid.NewGuid(), "document C.docx", "document C.docx", FileType.NDA, DateTimeOffset.Now.AddMonths(-1).AddDays(-3)),
        }
        .ToImmutable()
        .Concat(historicalFilesA)
        .Concat(historicalFilesB)
        .ToImmutable();

        var modifications = new List<Modification>
        {
            new Modification("Dolor hendrerit accumsan sit ut duo et nonumy hendrerit sed ut et invidunt tincidunt dolore duo. Justo aliquyam dolor facilisi ex in tempor sed augue. Erat amet diam lorem eu erat in sed eirmod aliquyam. Clita elitr eirmod est sit est. Nonumy aliquyam aliquip ipsum lorem amet sed justo odio enim in sit iusto ipsum ut in takimata nonumy minim. Nibh est clita magna consetetur nisl sadipscing takimata nam eleifend stet ut lorem magna feugiat sit amet amet nulla. Accusam nulla no. Sit eirmod hendrerit eum at dolor ea ea lobortis tempor commodo sit vel est at labore dolor. Luptatum est sadipscing takimata zzril iriure nulla accusam amet. Adipiscing elit takimata lorem sit dolores nonummy accusam ea magna sit. Invidunt est amet diam consequat sed lorem eos sed. Erat clita vero ipsum labore takimata sed amet ipsum lorem euismod diam labore.", DateTimeOffset.UtcNow.AddDays(-1).AddMinutes(-1)),
            new Modification("Amet lorem duo feugait justo et accusam justo dolor duo accusam rebum in elitr lorem et iusto. Eirmod praesent accusam sanctus sadipscing amet sanctus lorem ea sea ipsum erat aliquyam. Qui diam ut. Dolore et est vero sea dolores veniam ut adipiscing amet ipsum elitr ut ipsum ipsum vero vel. Lorem consetetur clita labore ipsum odio est euismod et dolores facilisi amet te. Et takimata nam eos erat vel sea est laoreet sea duo. Et placerat esse amet voluptua sanctus at consetetur eros takimata. Delenit sit illum invidunt ea amet duis dolor dolore invidunt invidunt luptatum ipsum nostrud congue amet lorem dolor vel. Lorem et dolore gubergren feugiat sanctus et hendrerit commodo elitr labore molestie justo. Takimata sed stet imperdiet euismod te sed nibh molestie at sanctus sed exerci nulla iriure sed. No sit lorem at elit nisl vero iusto sadipscing eos et iriure diam vel lorem kasd.", DateTimeOffset.UtcNow.AddDays(-2).AddMinutes(-2)),
            new Modification("Suscipit mazim in aliquam ipsum sed aliquam consetetur dignissim diam clita invidunt dignissim takimata eirmod aliquyam. Nihil nonumy ut magna diam et sit takimata diam amet invidunt euismod cum vel et delenit. Dolor eirmod amet facilisi zzril et consequat sit no stet id vulputate. No nonumy et sea ut nulla molestie stet ipsum.", DateTimeOffset.UtcNow.AddDays(-3).AddMinutes(-3))
        }.ToImmutable();

        Submissions = new List<DealSubmission>
        {
            new DealSubmission(Guid.Parse("d5486eb8-aa29-43cf-9a10-febbee7ed9d2"), "Project Bravo", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.Create<Assignee>(), ImmutableList.Create<DealFile>(), ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(-1)),
            new DealSubmission(Guid.Parse("3b2e3ccb-356d-429f-a182-e15271740d26"), "Project Power", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(1000000, Currency.GBP), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees.Take(2)), files, ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(1)),
            new DealSubmission(Guid.Parse("5a2c8167-094d-4909-b293-7b229aa67208"), "Project Lilypad", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(150000000, Currency.GBP), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees), files, ImmutableList.Create<FeedbackDetails>(), modifications, DateTimeOffset.Now.AddDays(2)),
            new DealSubmission(Guid.Parse("cd01013d-9996-40fe-8ea0-32878a1f66f0"), "Project Bazooka", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(22000000, Currency.EUR), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees.Take(3)), files, ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(3)),
            new DealSubmission(Guid.Parse("058c0b6e-97c0-4d46-b477-415b5a842ca3"), "Project Reflect", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(33000000, Currency.GBP), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees.Take(4)), files, ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(4)),
            new DealSubmission(Guid.Parse("2fdf39bc-3558-4450-8886-6eae180b2ad7"), "Project Turbo", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(120000000, Currency.GBP), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees), ImmutableList.Create<DealFile>(), GetFeedbackIdsForSubmission(Guid.Parse("2fdf39bc-3558-4450-8886-6eae180b2ad7"), feedbackIds), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(5)),
            new DealSubmission(Guid.Parse("a4499460-2f4f-47a2-ad50-841fb91ab343"), "Project Hyperspeed", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(1000000, Currency.GBP), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees.Take(2)), files, GetFeedbackIdsForSubmission(Guid.Parse("a4499460-2f4f-47a2-ad50-841fb91ab343"), feedbackIds), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(6)),
            new DealSubmission(Guid.Parse("f43c2203-a191-4485-94d9-a95ac91fb90b"), "Project Wow", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(150000000, Currency.USD), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees), files, GetFeedbackIdsForSubmission(Guid.Parse("f43c2203-a191-4485-94d9-a95ac91fb90b"), feedbackIds), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(7)),
            new DealSubmission(Guid.Parse("abac8c37-3e26-4bca-a51c-a9fc66de11a0"), "Project Gigantic", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(22000000, Currency.EUR), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees.Take(3)), files, GetFeedbackIdsForSubmission(Guid.Parse("abac8c37-3e26-4bca-a51c-a9fc66de11a0"), feedbackIds), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(8)),
            new DealSubmission(Guid.Parse("b3c4faa2-6511-4985-b849-5fb590c51e5f"), "Project Eternity", "BrokerCo", brokerCompanyId, terms, new SubmissionPricing(33000000, Currency.GBP), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.CreateRange(dev_assignees.Take(4)), files, GetFeedbackIdsForSubmission(Guid.Parse("b3c4faa2-6511-4985-b849-5fb590c51e5f"), feedbackIds), ImmutableList.Create<Modification>(), DateTimeOffset.Now),
        };
    }
    private IImmutableList<FeedbackDetails> GetFeedbackIdsForSubmission(Guid submissionId, List<(Guid insurerId, Guid submissionId, Guid feedbackId)> feedbackIds) => 
        feedbackIds.Where(i => i.submissionId == submissionId).Select(i => FeedbackDetails.Factory.Create(i.feedbackId, i.insurerId)).ToImmutable();    

    private void SeedFeedbacks(Guid insurerCompanyId, List<(Guid insurerId, Guid submissionId, Guid feedbackId)> feedbackIds, bool submitted, bool declined)
    {
        var randomGenerator = new Random();

        var pricing = new FeedbackPricing(
            new Money(100000000, Currency.GBP),
            new Money(randomGenerator.Next(10000, 40000), Currency.GBP),
            new Money(randomGenerator.Next(10000, 40000), Currency.GBP),
            true,
            new Money(randomGenerator.Next(10000, 40000), Currency.GBP),
            true,
            new List<PricingOption>
            {
                new PricingOption(new Limit(0, 0.10, true), new Retention(0.005, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(0, 0.10, true), new Retention(0.0035, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(0, 0.10, true), new Retention(0.0025, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(0, 0.10, true), new Retention(0, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(1, 0.15, true), new Retention(0.005, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(1, 0.15, true), new Retention(0.0035, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(1, 0.15, true), new Retention(0.0025, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(1, 0.15, true), new Retention(0, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(2, 0.20, true), new Retention(0.005, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(2, 0.20, true), new Retention(0.0035, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(2, 0.20, true), new Retention(0.0025, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(2, 0.20, true), new Retention(0, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(3, 0.25, true), new Retention(0.005, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(3, 0.25, true), new Retention(0.0035, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(3, 0.25, true), new Retention(0.0025, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
                new PricingOption(new Limit(3, 0.25, true), new Retention(0, true), new Money(randomGenerator.Next(80000, 200000), Currency.GBP)),
            }
            );

        var uwFocus = new List<string> { "Compliance", "Permits", "Regulatory", "Environmental", "Cyber" }.ToImmutable();
        var excludedCountries = new List<string> { "China", "Ukraine" }.ToImmutable();
        var warranties = Warranty.Factory.Default
            .Select(w => 
            new Warranty(
                w.Order, 
                w.Description, 
                (CoveragePosition)randomGenerator.Next(0, 4), 
                (KnowledgeScrape)randomGenerator.Next(0, 3),
                randomGenerator.Next(3) == 1 ? "Some loooooooong comment, that goes on and on and on and on and on and on and on and on and on and on and on and on and on and on and on and on" : string.Empty))
            .ToImmutable();
        var exclusions = Exclusion.Factory.Default
            .Select(e => 
            new Exclusion(
                e.Title,
                e.Description,
                e.Comment,
                randomGenerator.Next(2) == 1
                )
            )
            .ToImmutable();
        var enhancements = Enhancement.Factory.Default
            .Select(e =>
            new Enhancement(
                e.Type,
                e.Title,
                e.Description,
                e.Comment,
                e.AP,
                randomGenerator.Next(2) == 1,
                randomGenerator.Next(2) == 1
                )
            )
            .ToImmutable();
        var notes = new string[] 
        {
            "At in elitr. Aliquip at et eos lorem lorem vero. Voluptua diam eirmod veniam. Diam ea sadipscing vero soluta no stet lorem consetetur nonumy magna dolores takimata at duis diam. Commodo rebum clita eu eos eum dignissim ipsum facilisi diam nam rebum vero sea autem eirmod tempor. Te accumsan dolor voluptua aliquyam sed ad tation est amet at magna consetetur sed kasd ut aliquyam ut praesent.",
            "In takimata dolor amet vel accumsan nonumy et erat dignissim nonumy invidunt accumsan ipsum amet eos. Labore labore stet molestie nonumy justo eos hendrerit commodo. Diam nonumy erat eirmod lorem ipsum eirmod et ut lorem magna ad justo hendrerit tempor vero magna dolor facilisis. Nibh sed stet et eos wisi no luptatum sit. Vero eirmod aliquyam nihil odio magna ipsum ut vero accusam dolor et ea justo. Ipsum dolor et ipsum ea tempor voluptua dolor dolor aliquam. Diam accumsan duis consequat hendrerit et.",
            "Lorem voluptua dolor et est eos lorem consequat et vero dolores in augue amet voluptua clita nonumy eirmod. Velit ipsum nihil takimata dolore erat erat tempor nostrud. Eirmod sit veniam aliquyam justo molestie vel. Et et vero kasd odio ipsum sanctus magna at sed dolore delenit consequat dolor. In ut diam dolor dignissim et diam gubergren justo voluptua. Labore iriure tempor stet te at sed. Et eos dolor. Minim takimata illum amet lorem vel aliquip stet est et amet diam erat."
        };

        var feedbackIdsForInsurer = feedbackIds.Where(i => i.insurerId == insurerCompanyId).ToList();
        var insurerCompanyName = $"Insurer {insurerCompanyId.ToString().Split('-').Last()}";
        Feedbacks.AddRange(new List<SubmissionFeedback>
        {
            new SubmissionFeedback(feedbackIdsForInsurer[0].feedbackId, feedbackIdsForInsurer[0].submissionId, insurerCompanyId, insurerCompanyName, true, submitted, declined, false, false, "Project Turbo", notes[randomGenerator.Next(0, 2)], pricing, enhancements, exclusions, excludedCountries, uwFocus, warranties),
            new SubmissionFeedback(feedbackIdsForInsurer[1].feedbackId, feedbackIdsForInsurer[1].submissionId, insurerCompanyId, insurerCompanyName, true, submitted, declined, false, false, "Project Hyperspeed", notes[randomGenerator.Next(0, 2)], pricing, enhancements, exclusions, excludedCountries, uwFocus, warranties),
            new SubmissionFeedback(feedbackIdsForInsurer[2].feedbackId, feedbackIdsForInsurer[2].submissionId, insurerCompanyId, insurerCompanyName, true, submitted, declined, false, false, "Project Wow", string.Empty, pricing, enhancements, exclusions, excludedCountries, uwFocus, warranties),
            new SubmissionFeedback(feedbackIdsForInsurer[3].feedbackId, feedbackIdsForInsurer[3].submissionId, insurerCompanyId, insurerCompanyName, true, submitted, declined, false, false, "Project Gigantic", notes[randomGenerator.Next(0, 2)], new FeedbackPricing(), enhancements, exclusions, excludedCountries, uwFocus, warranties),
            new SubmissionFeedback(feedbackIdsForInsurer[4].feedbackId, feedbackIdsForInsurer[4].submissionId, insurerCompanyId, insurerCompanyName, true, submitted, declined, false, false, "Project Eternity", notes[randomGenerator.Next(0, 2)], pricing, enhancements, exclusions, excludedCountries, uwFocus, warranties),
        });
    }
}
