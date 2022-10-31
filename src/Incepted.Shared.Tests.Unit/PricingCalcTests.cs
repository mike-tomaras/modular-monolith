using FluentAssertions;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System.Collections.Generic;

namespace Incepted.Shared.Tests.Unit;

[TestFixture]
public class PricingCalcTests
{
    [TestCase(1000000, 1000, 0.1, 1)]
    [TestCase(1000000, 0, 0.1, 0)]
    [TestCase(1000000, 1000, 0, 0)]
    [TestCase(0, 1000, 0.1, 0)]
    public void WhenGettingTheRoL(decimal evAmount, decimal premiumAmount, double limitValue, decimal expectedResult)
    {
        //Arrange
        var ev = new Money(evAmount, Currency.GBP);
        var premium = new Money(premiumAmount, Currency.GBP);
        var limit = new Limit(0, limitValue, true);

        //Act 
        var result = PricingCalc.RoL(ev, premium, limit);

        //Assert
        result.Should().Be(expectedResult);
    }

    [TestCase(1000, 0.05, 0.05, 0.05, 150)]
    [TestCase(0, 0.05, 0.05, 0.05, 0)]
    [TestCase(1000, 0, 0, 0, 0)]
    public void WhenGettingTheEnhancementValue(decimal premiumAmount, double enhancementValue1, double enhancementValue2, double enhancementValue3, decimal expectedResult)
    {
        //Arrange
        var premium = new Money(premiumAmount, Currency.GBP);
        var enhancements = new List<Enhancement>
        {
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue1, true, true),
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue2, true, true),
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue3, true, true),
        };

        //Act 
        var result = PricingCalc.EnhancementValue(premium, enhancements);

        //Assert
        result.Should().Be(expectedResult);
    }

    [TestCase(1000, 0.05, 0.05, 0.05, "150")]
    [TestCase(3333, 0.01, 0.033, 0.05, "309.97")]
    [TestCase(0, 0.05, 0.05, 0.05, "n/a")]
    public void WhenGettingTheEnhancementString(decimal premiumAmount, double enhancementValue1, double enhancementValue2, double enhancementValue3, string expectedResult)
    {
        //Arrange
        var premium = new Money(premiumAmount, Currency.GBP);
        var enhancements = new List<Enhancement>
        {
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue1, true, true),
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue2, true, true),
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue3, true, true),
        };

        //Act 
        var result = PricingCalc.EnhancementValueString(premium, enhancements);

        //Assert
        result.Should().Be(expectedResult);
    }

    [TestCase(1000, 1000, 0.05, 0.05, 0.05, 2150)]
    [TestCase(0, 1000, 0.05, 0.05, 0.05, 1000)]
    [TestCase(1000, 0, 0.05, 0.05, 0.05, 1150)]
    [TestCase(1000, 1000, 0, 0, 0, 2000)]
    [TestCase(0, 0, 0.05, 0.05, 0.05, 0)]
    public void WhenGettingTheTotalValue(decimal premiumAmount, decimal uwFeeAmount, double enhancementValue1, double enhancementValue2, double enhancementValue3, decimal expectedResult)
    {
        //Arrange
        var premium = new Money(premiumAmount, Currency.GBP);
        var uwFee = new Money(uwFeeAmount, Currency.GBP);
        var enhancements = new List<Enhancement>
        {
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue1, true, true),
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue2, true, true),
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue3, true, true),
        };

        //Act 
        var result = PricingCalc.Total(premium, enhancements, uwFee);

        //Assert
        result.Should().Be(expectedResult);
    }

    [TestCase(1000, 1000, 0.05, 0.05, 0.05, "2150")]
    [TestCase(3333, 1000, 0.01, 0.033, 0.05, "4642.97")]
    [TestCase(0, 0, 0.05, 0.05, 0.05, "N/A")]
    public void WhenGettingTheTotalString(decimal premiumAmount, decimal uwFeeAmount, double enhancementValue1, double enhancementValue2, double enhancementValue3, string expectedResult)
    {
        //Arrange
        var premium = new Money(premiumAmount, Currency.GBP);
        var uwFee = new Money(uwFeeAmount, Currency.GBP);
        var enhancements = new List<Enhancement>
        {
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue1, true, true),
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue2, true, true),
            new Enhancement(EnhancementType.Request, "title", "desc", string.Empty, enhancementValue3, true, true),
        };

        //Act 
        var result = PricingCalc.TotalString(premium, enhancements, uwFee);

        //Assert
        result.Should().Be(expectedResult);
    }
}
