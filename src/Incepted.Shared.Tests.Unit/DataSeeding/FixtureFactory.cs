using AutoFixture;
using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.ValueTypes;

namespace Incepted.Shared.Tests.Unit.DataSeeding;

public static class FixtureFactory
{
    public static Fixture Create()
    {
        var fixture = new Fixture();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        fixture.Customize<UserId>(composer => composer.FromFactory(new UserIdSpecimenBuilder()));
        fixture.Customize<DealFile>(composer => composer.FromFactory(new DealFileSpecimenBuilder()));
        fixture.Customize<CompanyFile>(composer => composer.FromFactory(new CompanyFileSpecimenBuilder()));
        fixture.Customize<Money>(composer => composer.FromFactory(new MoneySpecimenBuilder()));
        fixture.Customize<FeedbackPricing>(composer => composer.FromFactory(new FeedbackPricingSpecimenBuilder()));
        fixture.Customize<HumanName>(composer => composer.FromFactory(new HumanNameSpecimenBuilder()));
        fixture.Customizations.Add(new ImmutableListSpecimenBuilder());
        return fixture;
    }
}
