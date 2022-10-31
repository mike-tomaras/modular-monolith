using FluentAssertions;
using Incepted.Domain.Companies.Application;
using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Application;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Moq;
using NUnit.Framework;
using Optional;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Incepted.Domain.Deals.Tests.Unit.DealServiceTests;

public class WhenGettingAllDealsForAUser : BaseUnitTest
{
    [Test]
    public void ShouldReturnUserDeals()
    {
        //Arrange
        var company = DataGenerator.Company();
        var userId = company.Employees.First().UserId;
        var input = DataGenerator.DealSubmissions(company.Id);
        var expectedResult = input.Select(d => DealSubmission.Factory.ToListItemDTO(d));

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissions(company.Id, company.Type))
            .Returns(Task.FromResult(input.Select(DealSubmission.Factory.ToListItemDTO).ToImmutable().Some<IImmutableList<DealListItemDTO>, ErrorCode>()));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.GetSubmissionsQuery(userId);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(list =>
        {
            list.Should().HaveCount(expectedResult.Count());
            list.Should().BeEquivalentTo(expectedResult);
        });
    }
}

public class WhenGettingSubmissionDetails : BaseUnitTest
{
    [Test]
    public void GivenAValidDealId_ShouldReturnSomeSubmissionDetails()
    {
        //Arrange
        var company = DataGenerator.Company();
        var userId = company.Employees.First().UserId;
        var input = DataGenerator.DealSubmissions(company.Id).First();
        var expectedResult = DealSubmission.Factory.ToListItemDTO(input);

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissionDetails(expectedResult.Id))
            .Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(input)));        

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.GetSubmissionDetailsQuery(userId, expectedResult.Id);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(deal =>
        {
            deal.Id.Should().Be(expectedResult.Id);
            deal.Name.Should().Be(expectedResult.Name);
        });
    }
}

public class WhenGettingFeedbakDetails : BaseUnitTest
{
    [Test]
    public void GivenAValidDealId_ShouldReturnSomeFeedbackDetails()
    {
        //Arrange
        var company = DataGenerator.Company();
        var userId = company.Employees.First().UserId;
        var input = DataGenerator.SubmissionFeedbacks(company.Id).First();
        var expectedResult = SubmissionFeedback.Factory.ToDTO(input);

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetFeedbackDetails(company.Id, expectedResult.SubmissionId))
            .Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(input)));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.GetSubmissionFeedbackDetailsQuery(userId, expectedResult.SubmissionId);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(deal =>
        {
            deal.Id.Should().Be(expectedResult.Id);
            deal.Name.Should().Be(expectedResult.Name);
        });
    }
}

public class WhenGettingAllTheFeedbakDetailsOfASubmission : BaseUnitTest
{
    [Test]
    public void GivenAValidDealId_ShouldReturnSomeFeedbackDetails()
    {
        //Arrange
        var company = DataGenerator.Company();
        var userId = company.Employees.First().UserId;
        var input = DataGenerator.SubmissionFeedbacks(company.Id).ToImmutable();
        var expectedResult = input.Select(SubmissionFeedback.Factory.ToDTO);
        var submissionId = input.First().SubmissionId;

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetAllFeedbackDetails(submissionId))
            .Returns(Task.FromResult(Option.Some<IImmutableList<SubmissionFeedback>, ErrorCode>(input)));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.GetAllFeedbackDetailsOfSubmissionQuery(userId, submissionId);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(feedbacks => feedbacks.Should().BeEquivalentTo(expectedResult));
    }
}