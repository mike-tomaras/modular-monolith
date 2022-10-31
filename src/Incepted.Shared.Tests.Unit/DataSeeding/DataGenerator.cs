using AutoFixture;
using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Mail;

namespace Incepted.Shared.Tests.Unit.DataSeeding
{
    public static class DataGenerator
    {
        public static Fixture Fixture { get; private set; } = FixtureFactory.Create();

        public static Company Company(CompanyType type = CompanyType.Broker)
        {
            var companyId = Guid.NewGuid();
            var companyName = Fixture.Create<string>();
            var expectedEmployees = new List<Employee>()
            {
                Employee(),
                Employee(),
                Employee(),
                Employee(),
            }.ToImmutable();
            var tcs = new CompanyFile(Guid.NewGuid(), "TCs.docx", "abc.def", FileType.InsurerTCs, DateTimeOffset.Now);
            return new Company(companyId, companyName, type, expectedEmployees, tcs);
        }

        public static Employee Employee()
        {
            return new Employee(Guid.NewGuid(), Fixture.Create<UserId>(), Fixture.Create<HumanName>(), new MailAddress($"{Guid.NewGuid()}@company.com"));
        }

        public static IEnumerable<DealSubmission> DealSubmissions(Guid companyId = default(Guid), IEnumerable<Assignee>? assignees = null, IEnumerable<DealFile>? files = null)
        {
            if (companyId == default(Guid)) companyId = Guid.NewGuid();

            if (assignees == null)
            {
                assignees = new List<Assignee>
                {
                    Assignee(),
                    Assignee(),
                    Assignee(),
                };
            }

            if (files == null)
            {
                files = new List<DealFile>
                {
                    new DealFile(Guid.NewGuid(), $"{Fixture.Create<string>()}.pdf", $"{Fixture.Create<string>()}.pdf", FileType.SPA, DateTimeOffset.Now.AddDays(-1)),
                    new DealFile(Guid.NewGuid(), $"{Fixture.Create<string>()}.pdf", $"{Fixture.Create<string>()}.pdf", FileType.IM, DateTimeOffset.Now.AddDays(-2)),
                    new DealFile(Guid.NewGuid(), $"{Fixture.Create<string>()}.pdf", $"{Fixture.Create<string>()}.pdf", FileType.NDA, DateTimeOffset.Now.AddDays(-3)),
                };
            }

            var pricing = new SubmissionPricing(
                new Money(100000000, Currency.GBP),
                new List<Limit>
                { 
                    new Limit(0, 0.1, true),
                    new Limit(1, 0.2, true),
                    new Limit(2, 0.3, true),
                },
                new List<Retention>
                { 
                    new Retention(0.01, true),
                    new Retention(0.02, true),
                    new Retention(0, true),
                });
            
            var deals = new List<DealSubmission>
            {
                new DealSubmission(
                    Fixture.Create<Guid>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    companyId,
                    Fixture.Create<BasicTerms>(),
                    pricing,
                    Enhancement.Factory.Default,
                    Warranty.Factory.Default,
                    assignees.ToImmutable(),
                    files.ToImmutable(),
                    ImmutableList.Create<FeedbackDetails>(),
                    ImmutableList.Create<Modification>(),
                    DateTimeOffset.UtcNow.AddDays(7)
                ),
                new DealSubmission(
                    Fixture.Create<Guid>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    companyId,
                    Fixture.Create<BasicTerms>(),
                    pricing,
                    Enhancement.Factory.Default,
                    Warranty.Factory.Default,
                    assignees.ToImmutable(),
                    files.ToImmutable(),
                    ImmutableList.Create<FeedbackDetails>(),
                    ImmutableList.Create<Modification>(),
                    DateTimeOffset.UtcNow.AddDays(7)
                ),
                new DealSubmission(
                    Fixture.Create<Guid>(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    companyId,
                    Fixture.Create<BasicTerms>(),
                    pricing,
                    Enhancement.Factory.Default,
                    Warranty.Factory.Default,
                    assignees.ToImmutable(),
                    files.ToImmutable(),
                    ImmutableList.Create<FeedbackDetails>(),
                    ImmutableList.Create<Modification>(),
                    DateTimeOffset.UtcNow.AddDays(7)
                ),
            };

            return deals;
        }

        public static IEnumerable<SubmissionFeedback> SubmissionFeedbacks(Guid dealId, Guid insuranceCompanyId) =>
            SubmissionFeedbacks(dealId, new List<Guid> { insuranceCompanyId, Guid.NewGuid(), Guid.NewGuid()});
        public static IEnumerable<SubmissionFeedback> SubmissionFeedbacks(Guid dealId, List<Guid>? insuranceCompanyIds) =>
            SubmissionFeedbacks(dealId, null, insuranceCompanyIds);
        public static IEnumerable<SubmissionFeedback> SubmissionFeedbacks(Guid dealId = default(Guid), List<Guid>? feedbackIds = null, List<Guid>? insuranceCompanyIds = null)
        {
            if (feedbackIds == null || !feedbackIds.Any()) feedbackIds = Fixture.CreateMany<Guid>(3).ToList();
            if (insuranceCompanyIds == null || !insuranceCompanyIds.Any()) insuranceCompanyIds = Fixture.CreateMany<Guid>(3).ToList();
            if (dealId == default(Guid)) dealId = Guid.NewGuid();

            var feedbacks = new List<SubmissionFeedback>
            {
                new SubmissionFeedback(
                    feedbackIds[0],
                    dealId,
                    insuranceCompanyIds[0],
                    "Insurance Corp",
                    false,
                    false,
                    false,
                    false,
                    false,
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    FeedbackPricing(),
                    Enhancement.Factory.Default,
                    Exclusion.Factory.Default,
                    ImmutableList.Create<string>(),
                    ImmutableList.Create<string>(),
                    Warranty.Factory.Default
                    ),
                new SubmissionFeedback(
                    feedbackIds[1],
                    dealId,
                    insuranceCompanyIds[1],
                    "Insurance Corp",
                    false,
                    false,
                    false,
                    false,
                    false,
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    FeedbackPricing(),
                    Enhancement.Factory.Default,
                    Exclusion.Factory.Default,
                    ImmutableList.Create<string>(),
                    ImmutableList.Create<string>(),
                    Warranty.Factory.Default
                    ),
                new SubmissionFeedback(
                    feedbackIds[2],
                    dealId,
                    insuranceCompanyIds[2],
                    "Insurance Corp",
                    false,
                    false,
                    false,
                    false,
                    false,
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    FeedbackPricing(),
                    Enhancement.Factory.Default,
                    Exclusion.Factory.Default,
                    ImmutableList.Create<string>(),
                    ImmutableList.Create<string>(),
                    Warranty.Factory.Default
                    ),
            };

            return feedbacks;
        }

        public static FeedbackPricing FeedbackPricing(Currency currency = Currency.GBP)
        {
            return new FeedbackPricing(
                new Money(Fixture.Create<decimal>(), currency),
                new Money(Fixture.Create<decimal>(), currency),
                new Money(Fixture.Create<decimal>(), currency),
                true,
                new Money(Fixture.Create<decimal>(), currency),
                true,
                new List<PricingOption> 
                { 
                    PricingOption(0, 0.1, 0.02), 
                    PricingOption(0, 0.1, 0.03), 
                    PricingOption(0, 0.1, 0),
                    PricingOption(1, 0.2, 0.02),
                    PricingOption(1, 0.2, 0.03),
                    PricingOption(1, 0.2, 0),
                    PricingOption(2, 0.3, 0.02),
                    PricingOption(2, 0.3, 0.03),
                    PricingOption(2, 0.3, 0),
                }
                );
        }

        public static PricingOption PricingOption(int limitId, double limit, double retention, Currency currency = Currency.GBP)
        {
            return new PricingOption(
                new Limit(limitId, limit, true), 
                new Retention(retention, true), 
                new Money(Fixture.Create<decimal>(), currency));
        }

        public static Assignee Assignee()
        {
            return new Assignee(Guid.NewGuid(), Fixture.Create<UserId>(), Fixture.Create<HumanName>(), new MailAddress("email@email.com"));
        }
        public static EmployeeDTO EmployeeDTO()
        {
            return new EmployeeDTO(Guid.NewGuid(), Fixture.Create<UserId>().ToString(), Fixture.Create<string>(), Fixture.Create<string>(), "email@email.com");
        }

        public static LiveDeal LiveDeal(Guid brokerCompanyId = default(Guid), Guid insurerCompanyId = default(Guid))
        {
            if (brokerCompanyId == default(Guid)) brokerCompanyId = Guid.NewGuid();
            if (insurerCompanyId == default(Guid)) insurerCompanyId = Guid.NewGuid();

            return new LiveDeal(
                    Guid.NewGuid(),
                    Fixture.Create<string>(),
                    Fixture.Create<string>(),
                    brokerCompanyId,
                    Fixture.Create<Guid>(),
                    Fixture.Create<string>(),
                    insurerCompanyId,
                    Fixture.Create<Guid>(),
                    Fixture.CreateMany<Assignee>().ToImmutable(),
                    Fixture.CreateMany<Assignee>().ToImmutable(),
                    Fixture.Create<Money>()
                    );
        }
    }
}
