using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.Tests.Unit.DataSeeding;
using NUnit.Framework;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Incepted.Domain.Deals.Tests.Unit.DomainTests;

public class WhenCreatingALiveDeal : BaseUnitTest
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewLiveDeal()
    {
        //Arrange
        var expectedDeal = DataGenerator.LiveDeal();

        //Act
        var result = new LiveDeal(expectedDeal.Id, expectedDeal.Name, expectedDeal.BrokerName, expectedDeal.BrokerCompanyId, expectedDeal.SubmissionId, expectedDeal.InsurerName, expectedDeal.InsuranceCompanyId, expectedDeal.FeedbackId, expectedDeal.AssigneesBroker, expectedDeal.AssigneesInsurer, expectedDeal.EnterpriseValue);

        //Assert
        result.Should().BeEquivalentTo(expectedDeal);
    }

    [Test]
    public void GivenIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new LiveDeal(Guid.Empty, "name", "BrokerCo", Guid.NewGuid(), Guid.NewGuid(), "InsurerCo", Guid.NewGuid(), Guid.NewGuid(), ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Shared.ValueTypes.Money());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Deal Id can't be empty (Parameter 'LiveDeal id')");
    }

    [Test]
    public void GivenBrokerCompanyIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new LiveDeal(Guid.NewGuid(), "name", "BrokerCo", Guid.Empty, Guid.NewGuid(), "InsurerCo", Guid.NewGuid(), Guid.NewGuid(), ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Shared.ValueTypes.Money());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Broker Company Id can't be empty (Parameter 'LiveDeal brokerCompanyId')");
    }
    
    [Test]
    public void GivenSubmissionIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new LiveDeal(Guid.NewGuid(), "name", "BrokerCo", Guid.NewGuid(), Guid.Empty, "InsurerCo", Guid.NewGuid(), Guid.NewGuid(), ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Shared.ValueTypes.Money());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Submission Id can't be empty (Parameter 'LiveDeal submissionId')");
    }

    [Test]
    public void GivenInsurerCompanyIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new LiveDeal(Guid.NewGuid(), "name", "BrokerCo", Guid.NewGuid(), Guid.NewGuid(), "InsurerCo", Guid.Empty, Guid.NewGuid(), ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Shared.ValueTypes.Money());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Broker Company Id can't be empty (Parameter 'LiveDeal insuranceCompanyId')");
    }

    [Test]
    public void GivenFeedbackIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new LiveDeal(Guid.NewGuid(), "name", "BrokerCo", Guid.NewGuid(), Guid.NewGuid(), "InsurerCo", Guid.NewGuid(), Guid.Empty, ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Shared.ValueTypes.Money());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Feedback Id can't be empty (Parameter 'LiveDeal feedbackId')");
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new LiveDeal(Guid.NewGuid(), name, "BrokerCo", Guid.NewGuid(), Guid.NewGuid(), "InsurerCo", Guid.NewGuid(), Guid.NewGuid(), ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Shared.ValueTypes.Money());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Deal name can't be empty (Parameter 'LiveDeal name')");
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenBrokerNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new LiveDeal(Guid.NewGuid(), "name", name, Guid.NewGuid(), Guid.NewGuid(), "InsurerCo", Guid.NewGuid(), Guid.NewGuid(), ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Shared.ValueTypes.Money());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Broker name can't be empty (Parameter 'LiveDeal brokerName')");
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenInsurerNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new LiveDeal(Guid.NewGuid(), "name", "BrokerCo", Guid.NewGuid(), Guid.NewGuid(), name, Guid.NewGuid(), Guid.NewGuid(), ImmutableList.Create<Assignee>(), ImmutableList.Create<Assignee>(), new Shared.ValueTypes.Money());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Broker name can't be empty (Parameter 'LiveDeal insurerName')");
    }

    [Test]
    public void GivenAnEmptyLiveDeal_ShouldBeEmpty()
    {
        //Arrange

        //Act - in the past fails
        var SUT = new EmptyLiveDeal();


        //Assert
        SUT.Id.Should().NotBeEmpty();
        SUT.Name.Should().Be("Empty");
        SUT.BrokerCompanyId.Should().NotBeEmpty();
        SUT.BrokerName.Should().Be("Empty");
        SUT.InsuranceCompanyId.Should().NotBeEmpty();
        SUT.InsurerName.Should().Be("Empty");
    }

    [Test]
    public void WhenCreatingANewLiveDeal_ShouldContainTheCorrectData()
    {
        //Arrange
        var submission = DataGenerator.DealSubmissions().First();
        var brokerAssignees = DataGenerator.Fixture.CreateMany<Assignee>().ToImmutable();
        submission.UpdateAssignees(brokerAssignees, submission.BrokerCompanyId);
        var feedbackDetails = DataGenerator.Fixture.CreateMany<FeedbackDetails>().ToImmutable();
        submission.Submit(feedbackDetails, DateTimeOffset.Now.AddDays(7));
        var feedback = DataGenerator.SubmissionFeedbacks(
            submission.Id, 
            feedbackDetails.Select(f => f.FeedbackId).ToList(),
            feedbackDetails.Select(f => f.InsuranceCompanyId).ToList()).First();
        

        //Act - in the past fails
        var SUT = LiveDeal.Factory.Create(submission, feedback);

        //Assert
        SUT.Id.Should().NotBeEmpty();
        SUT.Name.Should().Be(submission.Name);
        SUT.BrokerName.Should().Be(submission.BrokerName);
        SUT.BrokerCompanyId.Should().Be(submission.BrokerCompanyId);
        SUT.SubmissionId.Should().Be(submission.Id);
        SUT.InsurerName.Should().Be(feedback.InsuranceCompanyName);
        SUT.InsuranceCompanyId.Should().Be(feedback.InsuranceCompanyId);
        SUT.FeedbackId.Should().Be(feedback.Id);
        SUT.AssigneesBroker.Should().BeEquivalentTo(submission.Assignees);
        SUT.AssigneesInsurer.Should().BeEquivalentTo(feedbackDetails.First().Assignees);
        SUT.EnterpriseValue.Should().Be(submission.Pricing.EnterpriseValue);
    }
}