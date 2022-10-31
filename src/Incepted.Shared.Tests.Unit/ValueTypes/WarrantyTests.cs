using AutoFixture;
using FluentAssertions;
using Incepted.Shared.Enums;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;

namespace Incepted.Shared.Tests.Unit.ValueTypes;


public class WhenCreatingAnWarranty
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewOne()
    {
        //Arrange
        var expectedOrder = DataGenerator.Fixture.Create<uint>();
        var expectedDesc = DataGenerator.Fixture.Create<string>();
        var expectedComment = DataGenerator.Fixture.Create<string>();
        var expectedCp = DataGenerator.Fixture.Create<CoveragePosition>();
        var expectedKs = DataGenerator.Fixture.Create<KnowledgeScrape>();

        //Act
        var result = new Warranty(expectedOrder, expectedDesc, expectedCp, expectedKs, expectedComment);

        //Assert
        result.Order.Should().Be(expectedOrder);
        result.Description.Should().Be(expectedDesc);
        result.Comment.Should().Be(expectedComment);
        result.CoveragePosition.Should().Be(expectedCp);
        result.KnowledgeScrape.Should().Be(expectedKs);

        //Act
        result = new Warranty(expectedOrder, expectedDesc);

        //Assert
        result.KnowledgeScrape.Should().Be(KnowledgeScrape.None);
        result.CoveragePosition.Should().Be(CoveragePosition.None);
        result.Comment.Should().BeEmpty();
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenDescIsEmpty_ShouldThrowArgumentException(string desc)
    {
        //Arrange

        //Act
        var action = () => new Warranty(1, desc, CoveragePosition.Yes, KnowledgeScrape.No, "comment");

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Warranty description can't be empty (Parameter 'Warranty description')");
    }

    [TestCase("something", true)]
    [TestCase("", false)]
    public void GivenCommentEmptyOrNot_ShouldReturnCorrectResultWhenAskedIfThereIsOne(string comment, bool expectedResult)
    {
        //Arrange
        var SUT = new Warranty(1, "desc", CoveragePosition.None, KnowledgeScrape.Yes, comment);

        //Act
        var result = SUT.HasComment;

        //Assert
        result.Should().Be(expectedResult);        
    }
}

public class WhenUpdatingAWarranty
{
    [Test]
    public void WhenUpdatingTheCoveragePosition_ShouldreturnANewIntanceWithTheUpdatedValue()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Warranty>();
        var expectedValue = CoveragePosition.TBC;

        //Act
        var result = SUT.SetCoveragePosition(expectedValue);

        //Assert
        result.CoveragePosition.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);
    }
    [Test]
    public void WhenUpdatingTheKnowledgeScrape_ShouldreturnANewIntanceWithTheUpdatedValue()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Warranty>();
        var expectedValue = KnowledgeScrape.Partial;

        //Act
        var result = SUT.SetKnowledgeScrape(expectedValue);

        //Assert
        result.KnowledgeScrape.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);
    }

    [Test]
    public void WhenUpdatingTheComment_ShouldreturnANewIntanceWithTheUpdatedValue()
    {
        //Arrange
        var expectedComment = "some comment";
        var SUT = DataGenerator.Fixture.Create<Enhancement>();
        
        //Act
        var result = SUT.SetComment(expectedComment);

        //Assert
        result.Comment.Should().Be(expectedComment);
        result.Should().NotBeSameAs(SUT);
    }    
}

