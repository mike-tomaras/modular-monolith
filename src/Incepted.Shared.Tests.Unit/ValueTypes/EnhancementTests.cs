using AutoFixture;
using FluentAssertions;
using Incepted.Shared.Enums;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;
using System.Linq;

namespace Incepted.Shared.Tests.Unit.ValueTypes;


public class WhenCreatingAnEnhancement
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewEnhancement()
    {
        //Arrange
        var expectedTitle = DataGenerator.Fixture.Create<string>();
        var expectedDesc = DataGenerator.Fixture.Create<string>();
        var expectedComment = DataGenerator.Fixture.Create<string>();
        var expectedBrokerSel = DataGenerator.Fixture.Create<bool>();
        var expectedInsurerSel = DataGenerator.Fixture.Create<bool>();
        var expectedType = DataGenerator.Fixture.Create<EnhancementType>();
        var expectedAP = 0.03;

        //Act
        var result = new Enhancement(expectedType, expectedTitle, expectedDesc, expectedComment, expectedAP, expectedBrokerSel, expectedInsurerSel);

        //Assert
        result.Title.Should().Be(expectedTitle);
        result.Description.Should().Be(expectedDesc);
        result.BrokerRequestsIt.Should().Be(expectedBrokerSel);
        result.InsurerOffersIt.Should().Be(expectedInsurerSel);
        result.Type.Should().Be(expectedType);
        result.Comment.Should().Be(expectedComment);
        result.AP.Should().Be(expectedAP);

        //Act
        result = new Enhancement(expectedType, expectedTitle, expectedDesc);

        //Assert
        result.BrokerRequestsIt.Should().Be(false);
        result.InsurerOffersIt.Should().Be(false);
        result.AP.Should().Be(0);
        result.Comment.Should().BeEmpty();
    }

    [Test]
    public void WhenGettingADefaultSetOfEnahncements_ShouldReturnTheCorrectSet()
    {
        //Arrange
        var expectedRequestsCount = 36;
        var expectedAssumptionsCount = 10;

        //Act
        var result = Enhancement.Factory.Default;

        //Assert
        result.Where(e => e.Type == EnhancementType.Request).Should().HaveCount(expectedRequestsCount);
        result.Where(e => e.Type == EnhancementType.Assumption).Should().HaveCount(expectedAssumptionsCount);
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenTitleOrDescIsEmpty_ShouldThrowArgumentException(string titleOrDesc)
    {
        //Arrange

        //Act
        var action = () => new Enhancement(EnhancementType.Request, titleOrDesc, "desc", "comment", 0, false, false);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Enhancement title can't be empty (Parameter 'Enhancement title')");

        //Act
        action = () => new Enhancement(EnhancementType.Request, "title", titleOrDesc, "comment", 0, false, false);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Enhancement description can't be empty (Parameter 'Enhancement description')");
    }

    [TestCase("something", true)]
    [TestCase("", false)]
    public void GivenCommentEmptyOrNot_ShouldReturnCorrectResultWhenAskedIfThereIsOne(string comment, bool expectedResult)
    {
        //Arrange
        var SUT = new Enhancement(EnhancementType.Request, "title", "desc", comment, 0, false, false);

        //Act
        var result = SUT.HasComment;

        //Assert
        result.Should().Be(expectedResult);        
    }

    [TestCase(0.05, true)]
    [TestCase(0, false)]
    public void GivenAPIsZeroOrNot_ShouldReturnCorrectResultWhenAskedIfThereIsOne(double ap, bool expectedResult)
    {
        //Arrange
        var SUT = new Enhancement(EnhancementType.Request, "title", "desc", "comment", ap, false, false);

        //Act
        var result = SUT.HasAP;

        //Assert
        result.Should().Be(expectedResult);
    }
}

public class WhenUpdatingAnEnhancement
{
    [Test]
    public void WhenUpdatingTheBrokerSelection_ShouldreturnANewIntanceWithTheUpdatedValue()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Enhancement>();
        var expectedValue = !SUT.BrokerRequestsIt;

        //Act
        var result = SUT.SetBrokerSelected(expectedValue);

        //Assert
        result.BrokerRequestsIt.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);
    }
    [Test]
    public void WhenUpdatingTheInsurerSelection_ShouldreturnANewIntanceWithTheUpdatedValue()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Enhancement>();
        var expectedValue = !SUT.InsurerOffersIt;

        //Act
        var result = SUT.SetInsurerSelected(expectedValue);

        //Assert
        result.InsurerOffersIt.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);
    }

    [TestCase(0.1, true)]
    [TestCase(00, false)]
    public void WhenUpdatingTheAP_ShouldreturnANewIntanceWithTheUpdatedValueAndSetInsurerToOfferIt(double expectedAp, bool expectedInsurerSelected)
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Enhancement>();
        
        //Act
        var result = SUT.SetAP(expectedAp * 100);

        //Assert
        result.MatchSome(enhancement => 
        {
            enhancement.AP.Should().Be(expectedAp);
            enhancement.InsurerOffersIt.Should().Be(expectedInsurerSelected);
            enhancement.Should().NotBeSameAs(SUT);
        });
    }

    [TestCase(-0.1)]
    [TestCase(1.1)]
    public void WhenUpdatingTheAP_GivenAnInvalidAP_ShouldReturnTheError(double expectedAp)
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Enhancement>();

        //Act
        var result = SUT.SetAP(expectedAp * 100);

        //Assert
        result.MatchNone(error => error.errors.name.First().Should().Be("Please enter a value between 0 and 100 for AP."));
    }
    [Test]
    public void WhenUpdatingTheAPToZero_GivenItsBrokerSelected_ShouldLeaveTheSelectionEnabled()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Enhancement>();
        SUT = SUT.SetBrokerSelected(true);

        //Act
        var result = SUT.SetAP(0);

        //Assert
        result.MatchSome(enhancement => enhancement.BrokerRequestsIt.Should().BeTrue());
    }

    [TestCase("some comment", true)]
    [TestCase("", false)]
    public void WhenUpdatingTheComment_ShouldreturnANewIntanceWithTheUpdatedValueAndSetInsurerToOfferIt(string expectedComment, bool expectedInsurerSelected)
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Enhancement>();
        
        //Act
        var result = SUT.SetComment(expectedComment);

        //Assert
        result.Comment.Should().Be(expectedComment);
        result.InsurerOffersIt.Should().Be(expectedInsurerSelected);
        result.Should().NotBeSameAs(SUT);
    }
    [Test]
    public void WhenUpdatingTheCommentToEmpty_GivenItsBrokerSelected_ShouldLeaveTheSelectionEnabled()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Enhancement>();
        SUT = SUT.SetBrokerSelected(true);

        //Act
        var result = SUT.SetComment(string.Empty);

        //Assert
        result.BrokerRequestsIt.Should().BeTrue();
    }
}

