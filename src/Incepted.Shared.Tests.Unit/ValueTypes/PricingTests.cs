using AutoFixture;
using FluentAssertions;
using Incepted.Shared.Enums;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Incepted.Shared.Tests.Unit.ValueTypes;

public class SubmissionPricingTests
{
    [Test]
    public void WhenCreatingAPricingyObj_GivenValidParams_ShouldReturnTheNewPricingObj()
    {
        //Arrange
        var expectedAmount = (ulong)1234567;
        var expectedCurrency = Currency.USD;

        //Act
        var SUT = new SubmissionPricing(expectedAmount, expectedCurrency);

        //Assert
        SUT.EnterpriseValue.Should().BeEquivalentTo(new Money(expectedAmount, expectedCurrency));
        SUT.Limits.Should().BeEquivalentTo(Limit.Factory.Default);
        SUT.Retentions.Should().BeEquivalentTo(Retention.Factory.Default);

        //Arrange
        var expectedEV = DataGenerator.Fixture.Create<Money>();
        var expectedLimits = DataGenerator.Fixture.CreateMany<Limit>().ToList();
        var expectedRetentions = DataGenerator.Fixture.CreateMany<Retention>().ToList();

        //Act
        SUT = new SubmissionPricing(expectedEV, expectedLimits, expectedRetentions);

        //Assert
        SUT.EnterpriseValue.Should().BeEquivalentTo(expectedEV);
        SUT.Limits.Should().BeEquivalentTo(expectedLimits);
        SUT.Retentions.Should().BeEquivalentTo(expectedRetentions);
    }

    [Test]
    public void WhenCreatingAPricingyObj_GivenNoLimits_ShouldThrow()
    {
        //Arrange
        var expectedEV = DataGenerator.Fixture.Create<Money>();
        var expectedRetentions = DataGenerator.Fixture.CreateMany<Retention>().ToList();

        //Act
        var action = () => new SubmissionPricing(expectedEV, new List<Limit>(), expectedRetentions);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Pricing must include at least one limit (Parameter 'SubmissionPricing limits')");

        //Act
        action = () => new SubmissionPricing(expectedEV, new List<Limit>() { new Limit(1, 0, true) }, expectedRetentions);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Pricing must include at least one limit (Parameter 'SubmissionPricing limits')");
    }    

    [Test]
    public void WhenCreatingAPricingyObj_GivenNoRetentions_ShouldThrow()
    {
        //Arrange
        var expectedEV = DataGenerator.Fixture.Create<Money>();
        var expectedLimits = DataGenerator.Fixture.CreateMany<Limit>().ToList();

        //Act
        var action = () => new SubmissionPricing(expectedEV, expectedLimits, new List<Retention>());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Pricing must include at least one retention (Parameter 'SubmissionPricing retentions')");

        //Act
        action = () => new SubmissionPricing(expectedEV, expectedLimits, new List<Retention>() { new Retention(0, true) });

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Pricing must include at least one retention (Parameter 'SubmissionPricing retentions')");
    }

    [Test]
    public void WhenCreatingAnEmptyPricingyObj_ShouldReturnTheNewPricingObj()
    {
        //Arrange

        //Act
        var SUT = new SubmissionPricing();

        //Assert
        SUT.EnterpriseValue.Should().BeEquivalentTo(new Money());
        SUT.Limits.Should().BeEquivalentTo(Limit.Factory.Default);
        SUT.Retentions.Should().BeEquivalentTo(Retention.Factory.Default);
    }

    [Test]
    public void WhenUpdatingTheEV_ShouldReturnTheNewPricingObj()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<SubmissionPricing>();
        var newAmount = (ulong)1111111111111;
        var newCurrency = Currency.GBP;

        //Act
        var result = SUT.SetEnterpriseValue(newAmount, newCurrency);

        //Assert
        result.Should().NotBe(SUT);
        result.EnterpriseValue.Should().BeEquivalentTo(new Money(newAmount, newCurrency));
    }

    [Test]
    public void WhenUpdatingTheLimits_ShouldReturnTheNewPricingObj()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<SubmissionPricing>();
        var newLimits = DataGenerator.Fixture.CreateMany<Limit>().ToList();

        //Act
        var result = SUT.SetLimits(newLimits);

        //Assert
        result.MatchSome(pricing => {
            pricing.Should().NotBeSameAs(SUT);
            pricing.Limits.Should().BeEquivalentTo(newLimits);
        });
    }

    [Test]
    public void WhenUpdatingTheLimits_GivenNoLimits_ShouldReturnErrorCode()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<SubmissionPricing>();

        //Act
        var result = SUT.SetLimits(new List<Limit>());

        //Assert
        result.MatchNone(error => {
            error.Should().Be(PricingErrorCodes.NoLimits);
        });

        //Act
        result = SUT.SetLimits(new List<Limit>() { new Limit(1, 0, true) });

        //Assert
        result.MatchNone(error => {
            error.Should().Be(PricingErrorCodes.NoLimits);
        });
    }

    [Test]
    public void WhenUpdatingTheRetentions_ShouldReturnTheNewPricingObj()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<SubmissionPricing>();
        var newRetentions = DataGenerator.Fixture.CreateMany<Retention>().ToList();

        //Act
        var result = SUT.SetRetentions(newRetentions);

        //Assert
        result.MatchSome(pricing => {
            pricing.Should().NotBeSameAs(SUT);
            pricing.Retentions.Should().BeEquivalentTo(newRetentions);
        });
    }

    [Test]
    public void WhenUpdatingTheRetentions_GivenAnNoRetentions_ShouldReturnErrorCode()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<SubmissionPricing>();

        //Act
        var result = SUT.SetRetentions(new List<Retention>());

        //Assert
        result.MatchNone(error => {
            error.Should().Be(PricingErrorCodes.NoRetentions);
        });

        //Act
        result = SUT.SetRetentions(new List<Retention>() { new Retention(0, true) });

        //Assert
        result.MatchNone(error => {
            error.Should().Be(PricingErrorCodes.NoRetentions);
        });
    }
}

public class MoneyTests
{
    [Test]
    public void WhenCreatingAMoneyObj_GivenValidParams_ShouldReturnTheNewMoneyObj()
    {
        //Arrange
        var expectedAmount = (ulong)1234567;
        var expectedCurrency = Currency.USD;

        //Act
        var result = new Money(expectedAmount, expectedCurrency);

        //Assert
        result.Amount.Should().Be(expectedAmount);
        result.Currency.Should().Be(expectedCurrency);

        //Act
        result = new Money();

        //Assert
        result.Amount.Should().Be(0);
        result.Currency.Should().Be(Currency.GBP);
    }

    [Test]
    public void WhenCreatingAMoneyObj_GivenNegativeAmount_ShouldThrowExc()
    {
        //Arrange
        var expectedAmount = -1234567m;
        var expectedCurrency = Currency.USD;

        //Act
        var action = () => new Money(expectedAmount, expectedCurrency);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Money amount can't be negative (Parameter 'Money amount')");
    }

    [Test]
    public void WhenGettingTheStringValue_ShouldReturnTheNumberAndCurrency()
    {
        //Arrange
        var expectedAmount = (ulong)1234567;
        var expectedCurrency = Currency.USD;
        var SUT = new Money(expectedAmount, expectedCurrency);

        //Act
        var result = SUT.ToString();

        //Assert
        result.Should().Be($"{expectedAmount.ToString("N2")} {expectedCurrency}");
    }

    [Test]
    public void WhenGettingTheStringValueOfNoMoney_ShouldReturnNotSet()
    {
        //Arrange
        var SUT = new Money();

        //Act
        var result = SUT.ToString();

        //Assert
        result.Should().Be($"Not set");
    }

    [Test]
    public void WhenSerializingAndDeserializing()
    {
        //Testing the [JsonConstructor] on the non-default ctor or Money class

        //Arrange
        var value = new Money(123456, Currency.EUR);

        //Act
        var json = JsonSerializer.Serialize(value);
        //Assert
        json.Should().Be("{\"amount\":123456,\"currency\":2}");

        //Act
        var result = JsonSerializer.Deserialize<Money>(json);
        //Assert
        result?.Amount.Should().Be(value.Amount);
        result?.Currency.Should().Be(value.Currency);
    }
}

public class LimitTests
{
    [Test]
    public void WhenCreatingALimitObj_GivenValidParams_ShouldReturnTheNewLimitObj()
    {
        //Arrange
        var expectedLimitId = DataGenerator.Fixture.Create<int>();
        var expectedLimit = DataGenerator.Fixture.Create<double>();
        var expectedEnabled = DataGenerator.Fixture.Create<bool>();
        
        //Act
        var result = new Limit(expectedLimitId, expectedLimit, expectedEnabled);

        //Assert
        result.Id.Should().Be(expectedLimitId);
        result.Value.Should().Be(expectedLimit);
        result.Enabled.Should().Be(expectedEnabled);
    }

    [Test]
    public void WhenCreatingALimitObj_GivenNegativeLimit_ShouldThrow()
    {
        //Arrange
        
        //Act
        var action = () => new Limit(1, -0.5, true);

        //Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Limit value can't be negative (Parameter 'Limit value')");
    }
    
    [Test]
    public void WhenGettingALimitAsAString_ShouldGetTheperCentWithTwoDecimals()
    {
        //Arrange
        var SUT = new Limit(1, 0.1122, true);

        //Act
        var result = SUT.ToString();

        //Assert
        result.Should().Be("11.22");

        //Arrange
        SUT = new Limit(1, 0.303, true);

        //Act
        result = SUT.ToString();

        //Assert
        result.Should().Be("30.3");
    }

    [Test]
    public void WhenApplyingTheLimitToAnAmount_ShouldGetCorrectPercentOfTheAmount()
    {
        //Arrange
        var SUT = new Limit(1, 0.50, true);
        var amount = new Money(100000, Currency.GBP);

        //Act
        var result = SUT.ToAmountString(amount);

        //Assert
        result.Should().Be(50000.ToString("N0") + " GBP");

        //Arrange
        amount = new Money(0, Currency.GBP);

        //Act
        result = SUT.ToAmountString(amount);

        //Assert
        result.Should().Be("-");
    }

    [Test]
    public void WhenGettingTheDefaultSetOfLimits_ShouldReturnTheBasicFour()
    {
        //Arrange


        //Act
        var result = Limit.Factory.Default;

        //Assert
        result[0].Id.Should().Be(0);
        result[0].Value.Should().Be(0.1);
        result[0].Enabled.Should().Be(true);
        result[1].Id.Should().Be(1);
        result[1].Value.Should().Be(0.15);
        result[1].Enabled.Should().Be(true);
        result[2].Id.Should().Be(2);
        result[2].Value.Should().Be(0.2);
        result[2].Enabled.Should().Be(true);
        result[3].Id.Should().Be(3);
        result[3].Value.Should().Be(0.25);
        result[3].Enabled.Should().Be(true);
    }
}

public class RetentionTests
{
    [Test]
    public void WhenCreatingARetentionObj_GivenValidParams_ShouldReturnTheNewRetentionObj()
    {
        //Arrange
        var expectedRetention = DataGenerator.Fixture.Create<double>();
        
        //Act
        var result = new Retention(expectedRetention, true);

        //Assert
        result.Value.Should().Be(expectedRetention);
    }

    [Test]
    public void WhenCreatingARetentionObj_GivenNegativeRetention_ShouldThrow()
    {
        //Arrange

        //Act
        var action = () => new Retention(-0.5, true);

        //Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("Retention value can't be negative (Parameter 'Retention value')");
    }

    [Test]
    public void WhenGettingARetentionAsAString_ShouldGetTheperCentWithTwoDecimals()
    {
        //Arrange
        var SUT = new Retention(0.1122, true);

        //Act
        var result = SUT.ToString();

        //Assert
        result.Should().Be("11.22");

        //Arrange
        SUT = new Retention(0.303, true);

        //Act
        result = SUT.ToString();

        //Assert
        result.Should().Be("30.3");
    }

    [Test]
    public void WhenApplyingTheRetentionToAnAmount_ShouldGetCorrectPercentOfTheAmount()
    {
        //Arrange
        var SUT = new Retention(0.50, true);
        var amount = new Money(100000, Currency.GBP);

        //Act
        var result = SUT.ToAmountString(amount);

        //Assert
        result.Should().Be(50000.ToString("N0") + " GBP");

        //Arrange
        amount = new Money(0, Currency.GBP);

        //Act
        result = SUT.ToAmountString(amount);

        //Assert
        result.Should().Be("-");
    }

    [Test]
    public void WhenGettingTheDefaultSetOfRetentions_ShouldReturnTheBasicFour()
    {
        //Arrange


        //Act
        var result = Retention.Factory.Default;

        //Assert
        result[0].Value.Should().Be(0.005);
        result[1].Value.Should().Be(0.0035);
        result[2].Value.Should().Be(0.0025);
        result[3].Value.Should().Be(0);
    }
}

public class PricingOptionTests
{
    [Test]
    public void WhenCreatingAPricingOptionObj_GivenValidParams_ShouldReturnTheNewObj()
    {
        //Arrange
        var expectedLimit = DataGenerator.Fixture.Create<Limit>();
        var expectedPremium = DataGenerator.Fixture.Create<Money>();
        var expectedRetention = DataGenerator.Fixture.Create<Retention>();

        //Act
        var result = new PricingOption(expectedLimit, expectedRetention, expectedPremium);

        //Assert
        result.Limit.Should().Be(expectedLimit);
        result.Retention.Should().Be(expectedRetention);
        result.Premium.Should().Be(expectedPremium);
    }

    [Test]
    public void WhenSettingThePremium_ShouldReturnANewObjWithTheUpdatedPremium()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<PricingOption>();
        var expectedPremium = DataGenerator.Fixture.Create<Money>();

        //Act
        var result = SUT.SetPremium(expectedPremium.Amount);

        //Assert
        result.Premium.Amount.Should().Be(expectedPremium.Amount);
        result.Premium.Currency.Should().Be(SUT.Premium.Currency);
        result.Should().NotBeSameAs(SUT);
    }
}

public class FeedbackPricingTests
{
    [Test]
    public void WhenCreatingAFeedbackPricingObj_GivenValidParams_ShouldReturnTheNewObj()
    {
        //Arrange
        var expectedEv = DataGenerator.Fixture.Create<Money>();
        var expectedDeminimis = DataGenerator.Fixture.Create<Money>();
        var expectedUwFee = DataGenerator.Fixture.Create<Money>();
        var expectedBreakFee = DataGenerator.Fixture.Create<Money>();
        var expectedBreakFeeWaive = DataGenerator.Fixture.Create<bool>();
        var expectedUwFeeWaive = DataGenerator.Fixture.Create<bool>();
        var expectedOptions = DataGenerator.Fixture.CreateMany<PricingOption>().ToList();
        
        //Act
        var result = new FeedbackPricing(expectedEv, expectedDeminimis, expectedUwFee, expectedUwFeeWaive, expectedBreakFee, expectedBreakFeeWaive, expectedOptions);

        //Assert
        result.EnterpriseValue.Should().Be(expectedEv);
        result.DeMinimis.Should().Be(expectedDeminimis);
        result.UwFee.Should().Be(expectedUwFee);
        result.BreakFee.Should().Be(expectedBreakFee);
        result.UwFeeWaiveOption.Should().Be(expectedUwFeeWaive);
        result.BreakFeeWaiveOption.Should().Be(expectedBreakFeeWaive);
        result.Options.Should().BeEquivalentTo(expectedOptions);
    }

    [Test]
    public void WhenCreatingAnEmptyFeedbackPricingObj_ShouldReturnTheNewEmptyObj()
    {
        //Arrange
        
        //Act
        var result = new FeedbackPricing();

        //Assert
        result.DeMinimis.Amount.Should().Be(0);
        result.UwFee.Amount.Should().Be(0);
        result.BreakFee.Amount.Should().Be(0);
        result.UwFeeWaiveOption.Should().BeFalse();
        result.BreakFeeWaiveOption.Should().BeFalse();
        result.Options.Should().BeEmpty();
    }

    [Test]
    public void WhenGettingTheOptionsOfASpecificLimit_ShouldReturnOptionsWithThatLimitOnly()
    {
        //Arrange
        var options = new List<PricingOption>
        { 
            new PricingOption(new Limit(0, 0.10, false), new Retention(0, false), new Money()),
            new PricingOption(new Limit(0, 0.10, false), new Retention(0, false), new Money()),
            new PricingOption(new Limit(0, 0.10, false), new Retention(0, false), new Money()),
            new PricingOption(new Limit(1, 0.11, false), new Retention(0, false), new Money()),
            new PricingOption(new Limit(2, 0.12, false), new Retention(0, false), new Money()),
            new PricingOption(new Limit(3, 0.13, false), new Retention(0, false), new Money()),
        };
        var SUT = new FeedbackPricing(new Money(), new Money(), new Money(), false, new Money(), false, options);

        //Act
        var result = SUT.OptionsOfLimit(0);

        //Assert
        result.Should().HaveCount(3);
        result.All(o => o.Limit.Id == 0).Should().BeTrue();
    }

    [Test]
    public void WhenSettingAFee_ShouldReturnANewObjWithTheNewFee()
    {
        //Arrange
        var expectedValue = 999m;
        var SUT = DataGenerator.Fixture.Create<FeedbackPricing>();

        //Act
        var result = SUT.SetFee("De Minimis", expectedValue);

        //Assert
        result.DeMinimis.Amount.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);

        //Act
        result = SUT.SetFee("UW fee", expectedValue);

        //Assert
        result.UwFee.Amount.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);

        //Act
        result = SUT.SetFee("Break fee", expectedValue);

        //Assert
        result.BreakFee.Amount.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);

        //Act
        var action = () => SUT.SetFee("Invalid string", expectedValue);

        //Assert
        action.Should().Throw<NotImplementedException>()
            .WithMessage("Trying to edit a pricing value that has not been accounted for when assigning the dialog result");
    }

    [Test]
    public void WhenSettingAWaiver_ShouldReturnANewObjWithTheNewWaiver()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<FeedbackPricing>();
        var expectedValue = !SUT.UwFeeWaiveOption;

        //Act
        var result = SUT.SetWaive("UW fee", expectedValue);

        //Assert
        result.UwFeeWaiveOption.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);

        //Arrange
        expectedValue = !SUT.BreakFeeWaiveOption;

        //Act
        result = SUT.SetWaive("Break fee", expectedValue);

        //Assert
        result.BreakFeeWaiveOption.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);

        //Act
        var action = () => SUT.SetWaive("Invalid string", expectedValue);

        //Assert
        action.Should().Throw<NotImplementedException>()
            .WithMessage("Trying to edit a pricing value that has not been accounted for when assigning the dialog result");
    }

    [Test]
    public void WhenSettingThePremiumOfAnOption_ShouldReturnANewObjWithTheUpdatedOption()
    {
        //Arrange
        var SUT = DataGenerator.FeedbackPricing();
        var option = SUT.Options.Last();
        var expectedValue = DataGenerator.Fixture.Create<decimal>();

        //Act
        var result = SUT.SetPremium(option, expectedValue);

        //Assert
        result.Options.Should().HaveCount(9);
        result.Options.Last().Premium.Amount.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);
    }

    [Test]
    public void WhenParsingPricingFromASubmission_ShouldReturnANewObjWithTheCorrectFeedbackPricingOptionsAndEmptyFees()
    {
        //Arrange
        var pricing = DataGenerator.DealSubmissions().First().Pricing;
        

        //Act
        var result = FeedbackPricing.Factory.Parse(pricing);

        //Assert
        result.DeMinimis.Amount.Should().Be(0);
        result.DeMinimis.Currency.Should().Be(pricing.EnterpriseValue.Currency);
        result.UwFee.Amount.Should().Be(0);
        result.UwFee.Currency.Should().Be(pricing.EnterpriseValue.Currency);
        result.BreakFee.Amount.Should().Be(0);
        result.BreakFee.Currency.Should().Be(pricing.EnterpriseValue.Currency);
        result.Options.Should().HaveCount(pricing.Limits.Count * pricing.Retentions.Count);
        foreach (var limit in pricing.Limits)
        {
            foreach (var ret in pricing.Retentions)
            {
                result.Options.Any(o => o.Limit.Value == limit.Value && o.Retention.Value == ret.Value).Should().BeTrue();
            }
        }
    }
}