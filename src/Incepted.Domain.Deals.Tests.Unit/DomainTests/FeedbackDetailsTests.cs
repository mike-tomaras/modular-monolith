using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.Tests.Unit.DataSeeding;
using NUnit.Framework;
using System;
using System.Collections.Immutable;

namespace Incepted.Domain.Deals.Tests.Unit.DomainTests;

public class WhenCreatingAFeedbackDetail : BaseUnitTest
{
    [Test]
    public void GivenCorrectData_ShouldCreateAFeedbackDetail()
    {
        //Arrange
        var expectedInsurerId = Guid.NewGuid();
        var expectedFeedbackId = Guid.NewGuid();
        var expectedAssignees = DataGenerator.Fixture.CreateMany<Assignee>().ToImmutable();
        
        //Act
        var result = new FeedbackDetails(expectedFeedbackId, expectedInsurerId, true, expectedAssignees);

        //Assert
        result.FeedbackId.Should().Be(expectedFeedbackId);
        result.InsuranceCompanyId.Should().Be(expectedInsurerId);
        result.IsLive.Should().BeTrue();
        result.Assignees.Should().BeEquivalentTo(expectedAssignees);
    }

    [Test]
    public void GivenIdFeedbackIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new FeedbackDetails(Guid.Empty, Guid.NewGuid(), false, ImmutableList.Create<Assignee>());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Feedback detail feedback Id can't be empty (Parameter 'FeedbackDetails feedbackId')");
    }

    [Test]
    public void GivenIdInsurerIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new FeedbackDetails(Guid.NewGuid(), Guid.Empty, false, ImmutableList.Create<Assignee>());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Feedback detail insurer Id can't be empty (Parameter 'FeedbackDetails insuranceCompanyId')");
    }
}

public class WhenSelectingAFeedbackDetailToGoLive : BaseUnitTest
{
    [Test]
    public void ShouldCreateAFeedbackDetailWithTheLiveFlagSetToTrue()
    {
        //Arrange
        var expectedInsurerId = Guid.NewGuid();
        var expectedFeedbackId = Guid.NewGuid();
        var SUT = new FeedbackDetails(expectedFeedbackId, expectedInsurerId, false, ImmutableList.Create<Assignee>());

        //Act
        var result = SUT.GoLive();

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(feedbackDetail => 
        {
            feedbackDetail.FeedbackId.Should().Be(expectedFeedbackId);
            feedbackDetail.InsuranceCompanyId.Should().Be(expectedInsurerId);
            feedbackDetail.IsLive.Should().Be(true);
        });        
    }
}
