using AutoFixture;
using FluentAssertions;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;

namespace Incepted.Shared.Tests.Unit.ValueTypes;


public class WhenCreatingAnExclusion
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewExclusion()
    {
        //Arrange
        var expectedTitle = DataGenerator.Fixture.Create<string>();
        var expectedDesc = DataGenerator.Fixture.Create<string>();
        var expectedComment = DataGenerator.Fixture.Create<string>();
        var expectedInsurerSel = DataGenerator.Fixture.Create<bool>();
        

        //Act
        var result = new Exclusion(expectedTitle, expectedDesc, expectedComment, expectedInsurerSel);

        //Assert
        result.Title.Should().Be(expectedTitle);
        result.Description.Should().Be(expectedDesc);
        result.InsurerRequiresIt.Should().Be(expectedInsurerSel);
        result.Comment.Should().Be(expectedComment);

        //Act
        result = new Exclusion(expectedTitle, expectedDesc);

        //Assert
        result.InsurerRequiresIt.Should().Be(false);
        result.Comment.Should().BeEmpty();
    }

    [Test]
    public void WhenGettingADefaultSetOfExclusions_ShouldReturnTheCorrectSet()
    {
        //Arrange
        
        //Act
        var result = Exclusion.Factory.Default;

        //Assert
        result.Should().HaveCount(21);
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenTitleIsEmpty_ShouldThrowArgumentException(string title)
    {
        //Arrange

        //Act
        var action = () => new Exclusion(title, "desc", "comment", false);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Exclusion title can't be empty (Parameter 'Exclusion title')");
    }

    [TestCase("something", true)]
    [TestCase("", false)]
    public void GivenCommentEmptyOrNot_ShouldReturnCorrectResultWhenAskedIfThereIsOne(string comment, bool expectedResult)
    {
        //Arrange
        var SUT = new Exclusion("title", "desc", comment, false);

        //Act
        var result = SUT.HasComment;

        //Assert
        result.Should().Be(expectedResult);        
    }
}

public class WhenUpdatingAnExclusion
{   
    [Test]
    public void WhenUpdatingTheInsurerSelection_ShouldreturnANewIntanceWithTheUpdatedValue()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Exclusion>();
        var expectedValue = !SUT.InsurerRequiresIt;

        //Act
        var result = SUT.SetInsurerSelected(expectedValue);

        //Assert
        result.InsurerRequiresIt.Should().Be(expectedValue);
        result.Should().NotBeSameAs(SUT);
    }

    [TestCase("some comment", true)]
    [TestCase("", false)]
    public void WhenUpdatingTheComment_ShouldreturnANewIntanceWithTheUpdatedValueAndSetInsurerToRequiresIt(string expectedComment, bool expectedInsurerSelected)
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Exclusion>();
        
        //Act
        var result = SUT.SetComment(expectedComment);

        //Assert
        result.Comment.Should().Be(expectedComment);
        result.InsurerRequiresIt.Should().Be(expectedInsurerSelected);
        result.Should().NotBeSameAs(SUT);
    }
    [Test]
    public void WhenUpdatingTheCommentToEmpty_GivenItsBrokerSelected_ShouldLeaveTheSelectionEnabled()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Exclusion>();
        SUT = SUT.SetInsurerSelected(true);

        //Act
        var result = SUT.SetComment(string.Empty);

        //Assert
        result.InsurerRequiresIt.Should().BeTrue();
    }
}

