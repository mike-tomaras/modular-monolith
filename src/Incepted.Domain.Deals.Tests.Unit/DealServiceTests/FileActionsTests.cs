using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Companies.Application;
using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Application;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Moq;
using NUnit.Framework;
using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Incepted.Domain.Deals.Tests.Unit.DealServiceTests;


public class WhenUploadingFilesToADeal : BaseUnitTest
{
    [Test]
    public void GivenUploadSucceeds_ShouldUpdateTheDealAndReturnTheResults()
    {
        //Arrange
        var company = DataGenerator.Company();
        var userId = company.Employees.First().UserId;
        var deal = DataGenerator.DealSubmissions(company.Id).First();
        var newFilesThatWillSucceed = DataGenerator.Fixture.CreateMany<DealFile>(2)
            .Select(DealFile.Factory.ToDTO)
            .ToList();
        var expectedSuccededResults = newFilesThatWillSucceed
            .Select(f => new FileUploadResult(f, true, DataGenerator.Fixture.Create<ErrorCode>()))
            .ToList();
        var newFilesThatWillFail = DataGenerator.Fixture.CreateMany<DealFile>(2)
            .Select(DealFile.Factory.ToDTO)
            .ToList();
        var expectedFailedResults = newFilesThatWillSucceed
            .Select(f => new FileUploadResult(f, false, DataGenerator.Fixture.Create<ErrorCode>()))
            .ToList();
        var savedFiles = new List<DealFile>();

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissionDetails(deal.Id)).Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(deal)));
        repo.Setup(repo => repo.Update(It.IsAny<DealSubmission>())).Callback<DealSubmission>((deal) => savedFiles = deal.Files.ToList())
            .Returns(Task.FromResult(new Shared.Unit().Some<Shared.Unit, ErrorCode>()));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        fileService.Setup(service => service.UploadAsync(deal.Id, newFilesThatWillSucceed[0], It.IsAny<Stream>())).Returns(Task.FromResult(expectedSuccededResults[0]));
        fileService.Setup(service => service.UploadAsync(deal.Id, newFilesThatWillSucceed[1], It.IsAny<Stream>())).Returns(Task.FromResult(expectedSuccededResults[1]));
        fileService.Setup(service => service.UploadAsync(deal.Id, newFilesThatWillFail[0], It.IsAny<Stream>())).Returns(Task.FromResult(expectedFailedResults[0]));
        fileService.Setup(service => service.UploadAsync(deal.Id, newFilesThatWillFail[1], It.IsAny<Stream>())).Returns(Task.FromResult(expectedFailedResults[1]));

        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var files = newFilesThatWillSucceed.Concat(newFilesThatWillFail).Select(f => (f, Stream.Null));
        var result = SUT.UploadSubmissionFilesCommand(userId, deal.Id, files);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(uploadResults => uploadResults.Where(r => r.Uploaded).Should().BeEquivalentTo(expectedSuccededResults));
        savedFiles.Should().Contain(file => file.FileName == newFilesThatWillSucceed[0].FileName);
        savedFiles.Should().Contain(file => file.FileName == newFilesThatWillSucceed[1].FileName);
        savedFiles.Should().NotContain(file => file.FileName == newFilesThatWillFail[0].FileName);
        savedFiles.Should().NotContain(file => file.FileName == newFilesThatWillFail[1].FileName);
    }
}

public class WhenRemovingAFileFromADeal : BaseUnitTest
{
    [Test]
    public void GivenRemovalSucceeds_ShouldUpdateTheDeal()
    {
        //Arrange
        var company = DataGenerator.Company();
        var userId = company.Employees.First().UserId;
        var deal = DataGenerator.DealSubmissions(company.Id).First();
        var fileToRemove = deal.Files.First();

        var savedFiles = new List<DealFile>();

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissionDetails(deal.Id)).Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(deal)));
        repo.Setup(repo => repo.Update(It.IsAny<DealSubmission>())).Callback<DealSubmission>((deal) => savedFiles = deal.Files.ToList())
            .Returns(Task.FromResult(new Shared.Unit().Some<Shared.Unit, ErrorCode>()));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        fileService.Setup(service => service.DeleteAsync(deal.Id, fileToRemove.StoredFileName)).Returns(Task.FromResult(new Shared.Unit().Some<Shared.Unit, ErrorCode>()));

        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.DeleteSubmissionFileCommand(userId, deal.Id, fileToRemove.Id);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(result => result.Should().BeAssignableTo<Shared.Unit>());
        savedFiles.Should().NotContain(file => file.Id == fileToRemove.Id);
    }
}

public class WhenAcceptingADealsNda : BaseUnitTest
{
    [Test]
    public void GivenTheRepoSucceeds_ShouldSetTheNdaAsAccepted()
    {
        //Arrange            
        var company = DataGenerator.Company();
        var companyId = company.Id;
        var userId = company.Employees.First().UserId;
        var feedback = DataGenerator.SubmissionFeedbacks(company.Id).First();

        var acceptDto = new AcceptFileDTO(feedback.Id, feedback.SubmissionId, feedback.InsuranceCompanyId, new FileDTO(Guid.NewGuid(), "file.txt", "asdas.asd", FileType.NDA, DateTimeOffset.Now.AddDays(-2), "application/txt"));

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetFeedbackDetails(feedback.InsuranceCompanyId, feedback.SubmissionId))
            .Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(feedback)));
        repo
            .Setup(repo => repo.Update(It.Is<SubmissionFeedback>(d => d.Id == feedback.SubmissionId)))
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.AcceptFileCommand(userId, acceptDto);

        //Assert
        feedback.NdaAccepted.Should().BeTrue();

        repo.Verify(repo => repo.Update(It.Is<SubmissionFeedback>(d =>
            d.NdaAccepted == true
        )));
    }
}