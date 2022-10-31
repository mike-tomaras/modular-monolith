using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using Optional.Unsafe;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Incepted.Domain.Deals.Tests.Unit.DomainTests;

public class WhenCreatingASubmissionFeedback : BaseUnitTest
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewSubmissionFeedback()
    {
        //Arrange
        var expectedFeedback = DataGenerator.SubmissionFeedbacks().First();

        //Act
        var result = new SubmissionFeedback(expectedFeedback.Id, expectedFeedback.SubmissionId, expectedFeedback.InsuranceCompanyId, expectedFeedback.InsuranceCompanyName, expectedFeedback.NdaAccepted, expectedFeedback.Submitted, expectedFeedback.Declined, expectedFeedback.IsLive, expectedFeedback.ForReview, expectedFeedback.Name, expectedFeedback.Notes, expectedFeedback.Pricing, expectedFeedback.Enhancements, expectedFeedback.Exclusions, expectedFeedback.ExcludedCountries, expectedFeedback.UwFocus, expectedFeedback.Warranties);

        //Assert
        result.Should().BeEquivalentTo(expectedFeedback);
    }

    [Test]
    public void GivenIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new SubmissionFeedback(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), string.Empty, false, false, false, false, false, "name", "notes", new FeedbackPricing(), ImmutableList.Create<Enhancement>(), ImmutableList.Create<Exclusion>(), ImmutableList.Create<string>(), ImmutableList.Create<string>(), ImmutableList.Create<Warranty>());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Feedback Id can't be empty (Parameter 'SubmissionFeedback id')");
    }

    [Test]
    public void GivenSubmissionIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new SubmissionFeedback(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), string.Empty, false, false, false, false, false, "name", "notes", new FeedbackPricing(), ImmutableList.Create<Enhancement>(), ImmutableList.Create<Exclusion>(), ImmutableList.Create<string>(), ImmutableList.Create<string>(), ImmutableList.Create<Warranty>());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Feedback's Deal Id can't be empty (Parameter 'SubmissionFeedback submissionId')");
    }

    [Test]
    public void GivenInsuranceCompanyIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new SubmissionFeedback(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, string.Empty, false, false, false, false, false, "name", "notes", new FeedbackPricing(), ImmutableList.Create<Enhancement>(), ImmutableList.Create<Exclusion>(), ImmutableList.Create<string>(), ImmutableList.Create<string>(), ImmutableList.Create<Warranty>());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Insurance company Id can't be empty (Parameter 'SubmissionFeedback insuranceCompanyId')");
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenInsuranceCompanyNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new SubmissionFeedback(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), name, false, false, false, false, false, "name", "notes", new FeedbackPricing(), ImmutableList.Create<Enhancement>(), ImmutableList.Create<Exclusion>(), ImmutableList.Create<string>(), ImmutableList.Create<string>(), ImmutableList.Create<Warranty>());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Insureance company name can't be empty (Parameter 'SubmissionFeedback insuranceCompanyName')");
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new SubmissionFeedback(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "company", false, false, false, false, false, name, "notes", new FeedbackPricing(), ImmutableList.Create<Enhancement>(), ImmutableList.Create<Exclusion>(), ImmutableList.Create<string>(), ImmutableList.Create<string>(), ImmutableList.Create<Warranty>());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Deal name can't be empty (Parameter 'SubmissionFeedback name')");
    }

    [Test]
    public void WhenCreatingANewSubmissionFeedback_ShouldContainTheCorrectSubmissionDataAndNoFeedbackData()
    {
        //Arrange
        var insurerId = Guid.NewGuid();
        var submission = DataGenerator.DealSubmissions().First();
        var brokerEnhancements = DataGenerator.DealSubmissions().First().Enhancements
            .Take(5).Select(e => e.SetBrokerSelected(true));
        var restOfEnahancements = DataGenerator.DealSubmissions().First().Enhancements.Skip(5);
        var warranties = DataGenerator.Fixture.CreateMany<Warranty>().ToImmutable();
        submission = new DealSubmission(
                    submission.Id,
                    submission.Name,
                    submission.BrokerName,
                    submission.BrokerCompanyId,
                    submission.Terms,
                    submission.Pricing,
                    brokerEnhancements.Concat(restOfEnahancements).ToImmutable(),
                    warranties,
                    submission.Assignees,
                    submission.Files,
                    ImmutableList.Create<FeedbackDetails>(),
                    ImmutableList.Create<Modification>(),
                    DateTimeOffset.UtcNow.AddDays(7)
                );

        //Act - in the past fails
        var SUT = SubmissionFeedback.Factory.Create(insurerId, insurerId.ToString(), submission);

        //Assert
        SUT.SubmissionId.Should().Be(submission.Id);
        SUT.InsuranceCompanyId.Should().Be(insurerId);
        SUT.InsuranceCompanyName.Should().Be(insurerId.ToString());
        SUT.Name.Should().Be(submission.Name);
        SUT.Notes.Should().BeEmpty();
        SUT.Pricing.Should().BeEquivalentTo(FeedbackPricing.Factory.Parse(submission.Pricing));
        SUT.Enhancements.Should().HaveCount(5);
        SUT.Enhancements.Should().AllSatisfy(e => brokerEnhancements.Any(be => be.Title == e.Title));
        SUT.Exclusions.Should().BeEquivalentTo(Exclusion.Factory.Default);
        SUT.UwFocus.Should().BeEmpty();
        SUT.Warranties.Should().BeEquivalentTo(warranties);
        SUT.Submitted.Should().BeFalse();
        SUT.Declined.Should().BeFalse();
        SUT.IsLive.Should().BeFalse();
        SUT.ForReview.Should().BeFalse();
    }

    [Test]
    public void GivenAnEmptySubmissionFeedback_ShouldBeEmpty()
    {
        //Arrange

        //Act - in the past fails
        var SUT = new EmptyFeedback();


        //Assert
        SUT.Id.Should().NotBeEmpty();
        SUT.InsuranceCompanyId.Should().NotBeEmpty();
        SUT.Name.Should().Be("Empty");
        SUT.Notes.Should().BeEmpty();
        SUT.Pricing.DeMinimis.Amount.Should().Be(0);
        SUT.Pricing.UwFee.Amount.Should().Be(0);
        SUT.Pricing.BreakFee.Amount.Should().Be(0);
        SUT.Pricing.Options.Should().BeEmpty();
        SUT.Enhancements.Should().BeEquivalentTo(Enhancement.Factory.Default);
        SUT.Exclusions.Should().BeEquivalentTo(Exclusion.Factory.Default);
        SUT.UwFocus.Should().BeEmpty();
        SUT.Warranties.Should().BeEmpty();
    }
}

public class WhenGettingAFeedbackDetailDTOFromADealFeedback : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDTO()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().First();

        //Act
        var result = SubmissionFeedback.Factory.ToDTO(SUT);

        //Assert
        result.Id.Should().Be(SUT.Id);
        result.Name.Should().Be(SUT.Name);
        result.Notes.Should().Be(SUT.Notes);
        result.Pricing.Should().BeEquivalentTo(SUT.Pricing);
        result.Enhancements.Should().BeEquivalentTo(SUT.Enhancements);
        result.Exclusions.Should().BeEquivalentTo(SUT.Exclusions);
        result.UwFocus.Should().BeEquivalentTo(SUT.UwFocus);
        result.Warranties.Should().BeEquivalentTo(SUT.Warranties);
        SUT.Submitted.Should().Be(SUT.Submitted);
        SUT.Declined.Should().Be(SUT.Declined);
        SUT.IsLive.Should().Be(SUT.IsLive);
        SUT.ForReview.Should().Be(SUT.ForReview);
    }
}

public class WhenUpdatingTheDetailsOfASubmissionFeedback : BaseUnitTest
{
    [Test]
    public void GivenDetailsAreValid_ShouldUpdateDetailsAndReturnSome()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().Last();

        var newDetails = new SubmissionFeedback(
                        DataGenerator.Fixture.Create<Guid>(),
                        DataGenerator.Fixture.Create<Guid>(),
                        DataGenerator.Fixture.Create<Guid>(),
                        DataGenerator.Fixture.Create<string>(),
                        DataGenerator.Fixture.Create<bool>(),
                        DataGenerator.Fixture.Create<bool>(),
                        DataGenerator.Fixture.Create<bool>(),
                        DataGenerator.Fixture.Create<bool>(),
                        DataGenerator.Fixture.Create<bool>(),
                        DataGenerator.Fixture.Create<string>(),
                        DataGenerator.Fixture.Create<string>(),
                        DataGenerator.Fixture.Create<FeedbackPricing>(),
                        DataGenerator.Fixture.CreateMany<Enhancement>().ToImmutable(),
                        DataGenerator.Fixture.CreateMany<Exclusion>().ToImmutable(),
                        DataGenerator.Fixture.CreateMany<string>().ToImmutable(),
                        DataGenerator.Fixture.CreateMany<string>().ToImmutable(),
                        DataGenerator.Fixture.CreateMany<Warranty>().ToImmutable()
                    );

        //Act
        var result = SUT.Update(SubmissionFeedback.Factory.ToDTO(newDetails));

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d =>
        {
            d.Id.Should().NotBe(newDetails.Id);
            d.InsuranceCompanyId.Should().NotBe(newDetails.InsuranceCompanyId);
            d.Name.Should().NotBe(newDetails.Name);
            d.Notes.Should().Be(newDetails.Notes);
            d.Pricing.Should().BeEquivalentTo(newDetails.Pricing);
            d.Enhancements.Should().BeEquivalentTo(newDetails.Enhancements);
            d.Exclusions.Should().BeEquivalentTo(newDetails.Exclusions);
            d.ExcludedCountries.Should().BeEquivalentTo(newDetails.ExcludedCountries);
            d.UwFocus.Should().BeEquivalentTo(newDetails.UwFocus);
            d.Warranties.Should().BeEquivalentTo(newDetails.Warranties);
            d.ETag.Should().BeEquivalentTo(newDetails.ETag);
        });
    }
}

public class WhenAcceptingTheNdaOfASubmissionFeedback : BaseUnitTest
{
    [Test]
    public void GivenDetailsAreValid_ShouldUpdateDetailsAndReturnSome()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().Last();

        //Act
        var result = SUT.AcceptNda();

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d => d.NdaAccepted.Should().BeTrue());
    }
}

public class WhenSubmittingASubmissionFeedback : BaseUnitTest
{
    [Test]
    public void ShouldUpdateFlag()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().Last();

        //Act
        var result = SUT.Submit();

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d => d.Submitted.Should().BeTrue());
    }
}

public class WhenDeclinignASubmission : BaseUnitTest
{
    [Test]
    public void ShouldUpdateFlag()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().Last();
        SUT = SUT.Submit().ValueOrDefault();//even if it's submitted

        //Act
        var result = SUT.Decline();

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d =>
        {
            d.Declined.Should().BeTrue();
            d.Submitted.Should().BeFalse();
        });
    }
}

public class WhenTheUnderlyingSubmissionOfTheSubmissionFeedbackIsModified : BaseUnitTest
{
    [Test]
    public void GivenFeedbackIsSubmitted_ShouldMarkTheFeedbackForReview()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().Last();
        SUT = SUT.Submit().ValueOrDefault();

        //Act
        var result = SUT.SubmissionModified();

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d => d.ForReview.Should().BeTrue());
    }

    [Test]
    public void GivenFeedbackIsNotSubmitted_ShouldReturnError()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().Last();//it is not submitted by default


        //Act
        var result = SUT.SubmissionModified();

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.FailedToModifyFeedback_NotSubmittedYet.errors.name.First()));
    }
}

public class WhenGoinglive : BaseUnitTest
{
    [Test]
    public void GivenFeedbackIsSubmitted_ShouldMarkTheFeedbackLive()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().Last();
        SUT = SUT.Submit().ValueOrDefault();

        //Act
        var result = SUT.GoLive();

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(d => d.IsLive.Should().BeTrue());
    }

    [Test]
    public void GivenFeedbackIsNotSubmitted_ShouldReturnError()
    {
        //Arrange
        var SUT = DataGenerator.SubmissionFeedbacks().Last();//it is not submitted by default


        //Act
        var result = SUT.GoLive();

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be(DealErrorCodes.FailedToGoLive_NotSubmittedYet.errors.name.First()));
    }
}
