using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using Optional;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Incepted.Domain.Deals.Tests.Unit.DomainTests;

public class WhenCreatingADealSubmission : BaseUnitTest
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewDealSubmission()
    {
        //Arrange
        var expectedDeal = DataGenerator.DealSubmissions().First();

        //Act
        var result = new DealSubmission(expectedDeal.Id, expectedDeal.Name, expectedDeal.BrokerName, expectedDeal.BrokerCompanyId, expectedDeal.Terms, expectedDeal.Pricing, expectedDeal.Enhancements, expectedDeal.Warranties, expectedDeal.Assignees, expectedDeal.Files, expectedDeal.Feedbacks, expectedDeal.Modifications, expectedDeal.SubmissionDeadline);

        //Assert
        result.Should().BeEquivalentTo(expectedDeal);
        result.Files.Should().BeInDescendingOrder(f => f.LastModified);
    }

    [Test]
    public void GivenIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new DealSubmission(Guid.Empty, "name", "BrokerCo", Guid.NewGuid(), new BasicTerms(), new SubmissionPricing(), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.Create<Assignee>(), ImmutableList.Create<DealFile>(), ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.UtcNow.AddDays(7));

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Deal Id can't be empty (Parameter 'DealSubmission id')");
    }

    [Test]
    public void GivenBrokerCompanyIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new DealSubmission(Guid.NewGuid(), "name", "BrokerCo", Guid.Empty, new BasicTerms(), new SubmissionPricing(), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.Create<Assignee>(), ImmutableList.Create<DealFile>(), ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.UtcNow.AddDays(7));

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Broker Company Id can't be empty (Parameter 'DealSubmission brokerCompanyId')");
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new DealSubmission(Guid.NewGuid(), name, "BrokerCo", Guid.NewGuid(), new BasicTerms(), new SubmissionPricing(), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.Create<Assignee>(), ImmutableList.Create<DealFile>(), ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.UtcNow.AddDays(7));

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Deal name can't be empty (Parameter 'DealSubmission name')");
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenBrokerNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new DealSubmission(Guid.NewGuid(), "name", name, Guid.NewGuid(), new BasicTerms(), new SubmissionPricing(), Enhancement.Factory.Default, Warranty.Factory.Default, ImmutableList.Create<Assignee>(), ImmutableList.Create<DealFile>(), ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.UtcNow.AddDays(7));

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Broker name can't be empty (Parameter 'DealSubmission brokerName')");
    }

    [Test]
    public void GivenMoneyIsNull_ShouldCreateADealWithZeroAmount()
    {
        //Arrange


        //Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = new DealSubmission(Guid.NewGuid(), "name", "BrokerCo", Guid.NewGuid(), new BasicTerms(), null, Enhancement.Factory.Default, Warranty.Factory.Default, new List<Assignee>() { DataGenerator.Fixture.Create<Assignee>() }.ToImmutable(), ImmutableList.Create<DealFile>(), ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.UtcNow.AddDays(7));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        //Assert
        result.Pricing.EnterpriseValue.Amount.Should().Be(0);
    }

    [Test]
    public void GivenFilesIsNull_ShouldCreateADealWithNoFiles()
    {
        //Arrange

        //Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var result = new DealSubmission(Guid.NewGuid(), "name", "BrokerCo", Guid.NewGuid(), new BasicTerms(), new SubmissionPricing(), Enhancement.Factory.Default, Warranty.Factory.Default, new List<Assignee>() { DataGenerator.Fixture.Create<Assignee>() }.ToImmutable(), null, ImmutableList.Create<FeedbackDetails>(), ImmutableList.Create<Modification>(), DateTimeOffset.UtcNow.AddDays(7));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        //Assert
        result.Files.Should().NotBeNull();
        result.Files.Should().HaveCount(0);
    }

    [Test]
    public void GivenAnEmptyDealSubmission_ShouldBeEmpty()
    {
        //Arrange

        //Act - in the past fails
        var SUT = new EmptySubmission();


        //Assert
        SUT.Id.Should().NotBeEmpty();
        SUT.Name.Should().Be("Empty");
        SUT.Pricing.EnterpriseValue.Amount.Should().Be(0);
        SUT.Assignees.Should().HaveCount(1);
        SUT.Assignees.First().UserId.ToString().Should().EndWith("empty");
        SUT.Assignees.First().Name.First.Should().Be("Empty");
        SUT.Files.Should().BeEmpty();
    }
}

public class WhenGettingADealListItemDTOFromADealSubmission : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDTO()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().First();

        //Act
        var result = DealSubmission.Factory.ToListItemDTO(SUT);

        //Assert
        CompareDealEntityAndDTOBasics(SUT, result);
        result.SubmissionDeadline.Should().Be(SUT.SubmissionDeadline);
    }
}

public class WhenGettingADealDetailDTOFromADealSubmission : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDTO()
    {
        //Arrange
        var SUT1 = DataGenerator.DealSubmissions().First();

        //Act
        var result = DealSubmission.Factory.ToDTO(SUT1);

        //Assert
        CompareDealEntityAndDTOBasics(SUT1, result);
    }
}

public class WhenUpdatingTheAssigneesOfADealSubmission : BaseUnitTest
{
    private DealSubmission SUT;
    private IImmutableList<Assignee> newAssignees;

    [SetUp]
    public void Setup()
    {
        SUT = DataGenerator.DealSubmissions().First();
        newAssignees = new List<Assignee>
        {
            DataGenerator.Assignee(),
            DataGenerator.Assignee(),
            DataGenerator.Assignee(),
        }.ToImmutable();
    }

    [Test]
    public void GivenAssigneesAreValidAndAreFromTheBroker_ShouldUpdateBrokerAssigneesAndReturnSome()
    {
        //Arrange        
        

        //Act
        var result = SUT.UpdateAssignees(newAssignees, SUT.BrokerCompanyId);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d => d.Assignees.Should().BeEquivalentTo(newAssignees));
    }

    [Test]
    public void GivenAssigneesAreValidAndAreFromTheInsurerOfAFeedback_ShouldUpdateTheCorrectInsurerAssigneesAndReturnSome()
    {
        //Arrange        
        var feedbackDetails = DataGenerator.Fixture.CreateMany<FeedbackDetails>().ToImmutable();
        var selectedFeedback = feedbackDetails.Last();
        SUT.Submit(feedbackDetails, DateTimeOffset.Now.AddDays(7));

        //Act
        var result = SUT.UpdateAssignees(newAssignees, selectedFeedback.InsuranceCompanyId);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d => 
            d.Feedbacks
            .Single(f => f.InsuranceCompanyId == selectedFeedback.InsuranceCompanyId)
            .Assignees.Should().BeEquivalentTo(newAssignees)
        );
    }

    [Test]
    public void GivenTheCompanyIdDoesNotMatchBrokerOrInsurers_ShouldReturnNone()
    {
        //Arrange - with no feedbacks
        
        //Act
        var result = SUT.UpdateAssignees(newAssignees, Guid.NewGuid());

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.AssigneesCompanyDoesNotExistInSubmission.errors.name.First()));

        //Arrange - with feedbacks
        var feedbackDetails = DataGenerator.Fixture.CreateMany<FeedbackDetails>().ToImmutable();
        SUT.Submit(feedbackDetails, DateTimeOffset.Now.AddDays(7));

        //Act
        result = SUT.UpdateAssignees(newAssignees, Guid.NewGuid());

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.AssigneesCompanyDoesNotExistInSubmission.errors.name.First()));
    }
}

public class WhenUpdatingTheDetailsOfADealSubmission : BaseUnitTest
{
    private DealSubmission newDetails;

    [SetUp]
    public void Setup()
    {
        newDetails = DataGenerator.DealSubmissions().First();
    }


    [Test]
    public void GivenDetailsAreValid_ShouldUpdateDetailsAndReturnSome()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().Last();

        //Act
        var result = SUT.Update(DealSubmission.Factory.ToDTO(newDetails));

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d =>
        {
            d.Name.Should().NotBe(newDetails.Name);
            d.BrokerName.Should().NotBe(newDetails.BrokerName);
            d.Pricing.Should().BeEquivalentTo(newDetails.Pricing);
            d.Files.Should().BeEquivalentTo(newDetails.Files);
            d.SubmissionDeadline.Should().Be(newDetails.SubmissionDeadline);
            d.ETag.Should().Be(newDetails.ETag);
        });
    }

    [Test]
    public void GivenDeadlineIsInvalid_ShouldReturnNone()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().Last();
        var dto = new DealSubmissionDTO(newDetails.Id, newDetails.Name, SUT.BrokerName, newDetails.Terms, newDetails.Pricing, newDetails.Enhancements, newDetails.Warranties, ImmutableList.Create<EmployeeDTO>(), ImmutableList.Create<FileDTO>(), ImmutableList.Create<(Guid, Guid)>(), ImmutableList.Create<Modification>(), DateTimeOffset.Now.AddDays(-1), "etag");

        //Act
        var result = SUT.Update(dto);

        //Assert
        result.HasValue.Should().BeFalse();
        result.Should().Be(Option.None<DealSubmission, ErrorCode>(DealErrorCodes.FailedToUpdateDeal_InvalidDeadline));
    }
}

public class WhenRemovingAFileFromADealSubmission : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDealWithoutTheFile()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().First();
        var fileIdToRemove = SUT.Files.First().Id;

        //Act
        var result = SUT.RemoveFile(fileIdToRemove);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d =>
        {
            d.Files.Should().HaveCount(2);
            d.Files.Should().NotContain(f => f.Id == fileIdToRemove);
        });
    }
}

public class WhenSubmittingADealSubmission : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDealWithTheNewSubmissionDateAndInsurers()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().First();
        var expectedInsurerIds = DataGenerator.Fixture.CreateMany<FeedbackDetails>().ToImmutable();
        var expectedDeadline = DateTimeOffset.Now.AddDays(3);

        //Act
        var result = SUT.Submit(expectedInsurerIds, expectedDeadline);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d =>
        {
            d.Feedbacks.Should().BeEquivalentTo(expectedInsurerIds);
            d.SubmissionDeadline.Should().Be(expectedDeadline);
        });
    }

    [Test]
    public void GivenAnInvaliDeadline_ShouldReturnTheError()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().First();
        var expectedDeadline = DateTimeOffset.Now.AddDays(-3);

        //Act
        var result = SUT.Submit(DataGenerator.Fixture.CreateMany<FeedbackDetails>().ToImmutable(), expectedDeadline);

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.FailedToUpdateDeal_InvalidDeadline.errors.name.First()));
    }
}

public class WhenModifyingAnAlreadySubmittedDealSubmission : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDealWithTheNewModificationRecord()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().First();
        SUT = SUT.Submit(DataGenerator.Fixture.CreateMany<FeedbackDetails>().ToImmutable(), DateTimeOffset.Now.AddDays(3)).ValueOrDefault();
        var expectedModificationNotes = DataGenerator.Fixture.Create<string>();

        //Act
        var result = SUT.ModifySubmission(expectedModificationNotes);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d =>
            d.Modifications.Should().Contain(m =>
                m.Notes == expectedModificationNotes &&
                m.TimeStamp.Date == DateTimeOffset.UtcNow.Date
            )
        );
    }

    [Test]
    public void GivenNoModificationNotes_ShouldReturnTheError()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().First();


        //Act
        var result = SUT.ModifySubmission(string.Empty);

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.FailedToModifyDeal_NoNotes.errors.name.First()));
    }

    [Test]
    public void GivenTheDealIsNotSubmitted_ShouldReturnTheError()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().First();

        //Act
        var result = SUT.ModifySubmission("some notes");

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.FailedToModifyDeal_NotSubmittedYet.errors.name.First()));
    }
}

public class WhenGoingLiveWithADealSubmission : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDealWithTheCorrectStatus()
    {
        //Arrange
        var feedbackDetails = ImmutableList.CreateRange(
            new List<FeedbackDetails> {
                FeedbackDetails.Factory.Create(Guid.NewGuid(), Guid.NewGuid()),
                FeedbackDetails.Factory.Create(Guid.NewGuid(), Guid.NewGuid()),
                FeedbackDetails.Factory.Create(Guid.NewGuid(), Guid.NewGuid())
            }
            );
        var SUT = DataGenerator.DealSubmissions().First();
        SUT = SUT.Submit(feedbackDetails, DateTimeOffset.Now.AddDays(3)).ValueOrDefault();
        var selectedFeedback = feedbackDetails.First();

        //Act
        var result = SUT.GoLive(selectedFeedback.FeedbackId);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d =>
        {
            d.Feedbacks.Where(i => i.FeedbackId != selectedFeedback.FeedbackId).All(f => f.IsLive == false).Should().BeTrue();
            d.Feedbacks.Single(i => i.FeedbackId == selectedFeedback.FeedbackId).IsLive.Should().BeTrue();
        });
    }

    [Test]
    public void GivenNoAFeedbackIdThatDoesNotExist_ShouldReturnTheError()
    {
        //Arrange
        var feedbackDetails = DataGenerator.Fixture.CreateMany<FeedbackDetails>().ToImmutable();
        var SUT = DataGenerator.DealSubmissions().First();
        SUT = SUT.Submit(feedbackDetails, DateTimeOffset.Now.AddDays(3)).ValueOrDefault();


        //Act
        var result = SUT.GoLive(Guid.NewGuid());

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.FeedbackNotFound.errors.name.First()));
    }

    [Test]
    public void GivenTheDealIsNotSubmitted_ShouldReturnTheError()
    {
        //Arrange
        var SUT = DataGenerator.DealSubmissions().First();

        //Act
        var result = SUT.GoLive(Guid.NewGuid());

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.FailedToGoLive_NotSubmittedYet.errors.name.First()));
    }
}