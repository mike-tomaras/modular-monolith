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
using Incepted.Shared.ValueTypes;
using Moq;
using NUnit.Framework;
using Optional;
using Optional.Unsafe;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Incepted.Domain.Deals.Tests.Unit.DealServiceTests;

public class WhenAddingANewSubmission : BaseUnitTest
{
    [Test]
    public void GivenDefaultAssigneeIsFound_ShouldReturnSomeNewDealId()
    {
        //Arrange
        var company = DataGenerator.Company();
        var user = company.Employees.First();

        var newDealName = DataGenerator.Fixture.Create<string>();

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.Create(It.Is<DealSubmission>(deal => deal.Name == newDealName)))
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(user.UserId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.CreateSubmissionCommand(user.UserId, newDealName);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(id => id.Should().NotBe(Guid.Empty));
        repo.Verify(repo => repo.Create(It.Is<DealSubmission>(d =>
            d.Name == newDealName &&
            d.BrokerName == company.Name &&
            d.Pricing.EnterpriseValue.Amount == 0 &&
            d.Assignees.Count() == 0
        )));
    }
}

public class WhenUpdatingASubmission : BaseUnitTest
{
    [Test]
    public void GivenTheRepoSucceeds_ShouldReturnTheRepoResult()
    {
        //Arrange            
        var company = DataGenerator.Company();
        var companyId = company.Id;
        var userId = company.Employees.First().UserId;
        var existingDeal = DataGenerator.DealSubmissions(company.Id).First();
        var newDeal = new DealSubmission(
                existingDeal.Id,
                DataGenerator.Fixture.Create<string>(),
                DataGenerator.Fixture.Create<string>(),
                DataGenerator.Fixture.Create<Guid>(),
                DataGenerator.Fixture.Create<BasicTerms>(),
                DataGenerator.Fixture.Create<SubmissionPricing>(),
                Enhancement.Factory.Default,
                DataGenerator.Fixture.CreateMany<Warranty>().ToImmutable(),
                DataGenerator.Fixture.CreateMany<Assignee>().ToImmutable(),//add new ones to verify later that the originals do not change
                DataGenerator.Fixture.CreateMany<DealFile>().ToImmutable(),
                DataGenerator.Fixture.CreateMany<FeedbackDetails>().ToImmutable(),
                DataGenerator.Fixture.CreateMany<Modification>().ToImmutable(),
                DateTimeOffset.UtcNow.AddDays(7)
            );
        var updateDto = DealSubmission.Factory.ToDTO(newDeal);

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissionDetails(newDeal.Id))
            .Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(existingDeal)));
        repo
            .Setup(repo => repo.Update(It.Is<DealSubmission>(d => d.Id == newDeal.Id)))
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.UpdateSubmissionDetailsCommand(userId, updateDto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(deal =>
        {
            //checking that there is something returned from the update, no specific details as this is a mock             
            deal.Terms.Notes.Should().Be(newDeal.Terms.Notes);
        });
        repo.Verify(repo => repo.Update(It.Is<DealSubmission>(d =>
            d.Pricing.EnterpriseValue.Amount == newDeal.Pricing.EnterpriseValue.Amount &&
            d.Pricing.EnterpriseValue.Currency == newDeal.Pricing.EnterpriseValue.Currency
        )));
        repo.Verify(repo => repo.Update(It.Is<DealSubmission>(d =>
            d.Assignees.First().UserId == existingDeal.Assignees.First().UserId
        )));//Assignees should not be replaced
    }
}

public class WhenUpdatingAFeedback : BaseUnitTest
{
    [Test]
    public void GivenTheRepoSucceeds_ShouldReturnTheRepoResult()
    {
        //Arrange            
        var company = DataGenerator.Company();
        var companyId = company.Id;
        var userId = company.Employees.First().UserId;
        var existingFeedback = DataGenerator.SubmissionFeedbacks(company.Id).First();
        var newFeedback = new SubmissionFeedback(
                existingFeedback.Id,
                existingFeedback.SubmissionId,
                existingFeedback.InsuranceCompanyId,
                existingFeedback.InsuranceCompanyName,
                existingFeedback.NdaAccepted,
                existingFeedback.Submitted,
                existingFeedback.Declined,
                existingFeedback.IsLive,
                existingFeedback.ForReview,
                existingFeedback.Name,
                DataGenerator.Fixture.Create<string>(),
                DataGenerator.Fixture.Create<FeedbackPricing>(),
                Enhancement.Factory.Default,
                Exclusion.Factory.Default.Select(e => new Exclusion(e.Title, DataGenerator.Fixture.Create<string>())).ToImmutable(),
                DataGenerator.Fixture.CreateMany<string>().ToImmutable(),
                DataGenerator.Fixture.CreateMany<string>().ToImmutable(),
                Warranty.Factory.Default.Select(w => new Warranty(w.Order, w.Description, DataGenerator.Fixture.Create<CoveragePosition>(), DataGenerator.Fixture.Create<KnowledgeScrape>(), DataGenerator.Fixture.Create<string>())).ToImmutable()
            );
        var updateDto = SubmissionFeedback.Factory.ToDTO(newFeedback);

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetFeedbackDetails(newFeedback.InsuranceCompanyId, newFeedback.SubmissionId))
            .Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(existingFeedback)));
        repo
            .Setup(repo => repo.Update(It.Is<SubmissionFeedback>(d => d.Id == newFeedback.Id)))
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.UpdateSubmissionFeedbackCommand(userId, updateDto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(deal =>
        {
            //checking that there is something returned from the update, no specific details as this is a mock             
            deal.Notes.Should().Be(newFeedback.Notes);

        });
        repo.Verify(repo => repo.Update(It.Is<SubmissionFeedback>(d =>
            d.Notes == newFeedback.Notes
        )));
    }
}

public class WhenSubmittingTheFeedbackForASubmission : BaseUnitTest
{
    [Test]
    public void GivenTheRepoSucceeds_ShouldReturnTheRepoResult()
    {
        //Arrange
        var notificationTypeSent = NotificationType.None;
        var notifiedRecipients = ImmutableList.Create<RecipientDTO>();
        var notificationData = ImmutableDictionary<string, string>.Empty;
        var brokerCompany = DataGenerator.Company(CompanyType.Broker);
        var expectedRecipients = brokerCompany.Employees.Select(e => e.Email.Address);

        var insurerCompany = DataGenerator.Company();
        var companyId = insurerCompany.Id;
        var userId = insurerCompany.Employees.First().UserId;
        var deal = DataGenerator.DealSubmissions(brokerCompany.Id).First();
        var feedback = DataGenerator.SubmissionFeedbacks(deal.Id, insurerCompany.Id).First();
        var submitDto = new SubmitDealFeedbackDTO(feedback.Id, feedback.SubmissionId);

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetFeedbackDetails(insurerCompany.Id, submitDto.SubmissionId))
            .Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(feedback)));
        repo.Setup(repo => repo.GetSubmissionDetails(submitDto.SubmissionId))
            .Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(deal)));
        repo.Setup(repo => repo.Update(It.Is<SubmissionFeedback>(f => f.Id == feedback.Id && f.Submitted)))
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(insurerCompany));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(brokerCompany.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(brokerCompany.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();
        notificationService
            .Setup(service => service.SendNotification(It.IsAny<NotificationType>(), It.IsAny<IImmutableList<RecipientDTO>>(), It.IsAny<IImmutableDictionary<string, string>>()))
            .Callback<NotificationType, IImmutableList<RecipientDTO>, IImmutableDictionary<string, string>>((type, recipients, data) =>
            {
                notificationTypeSent = type;
                notifiedRecipients = (ImmutableList<RecipientDTO>)recipients;
                notificationData = (ImmutableDictionary<string, string>)data;
            })
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.SubmitFeedbackCommand(userId, submitDto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(unit => unit.Should().BeAssignableTo(typeof(Shared.Unit)));

        notificationTypeSent.Should().Be(NotificationType.Broker_NewSubmissionFeedback);
        notifiedRecipients.Select(r => r.Email).Should().BeEquivalentTo(expectedRecipients);
        notificationData["insurer_company"].Should().Be(insurerCompany.Name);
        notificationData["project_name"].Should().Be(deal.Name);
        notificationData["deal_id"].Should().Be(submitDto.SubmissionId.ToString());
    }
}

public class WhenDecliningASubmission : BaseUnitTest
{
    [Test]
    public void GivenTheRepoSucceeds_ShouldReturnTheRepoResult()
    {
        //Arrange
        var notificationTypeSent = NotificationType.None;
        var notifiedRecipients = ImmutableList.Create<RecipientDTO>();
        var notificationData = ImmutableDictionary<string, string>.Empty;
        var brokerCompany = DataGenerator.Company(CompanyType.Broker);
        var expectedRecipients = brokerCompany.Employees.Select(e => e.Email.Address);

        var insurerCompany = DataGenerator.Company();
        var companyId = insurerCompany.Id;
        var userId = insurerCompany.Employees.First().UserId;
        var deal = DataGenerator.DealSubmissions(brokerCompany.Id).First();
        var feedback = DataGenerator.SubmissionFeedbacks(deal.Id, insurerCompany.Id).First();
        var submitDto = new SubmitDealFeedbackDTO(feedback.Id, feedback.SubmissionId);

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetFeedbackDetails(insurerCompany.Id, submitDto.SubmissionId))
            .Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(feedback)));
        repo.Setup(repo => repo.GetSubmissionDetails(submitDto.SubmissionId))
            .Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(deal)));
        repo.Setup(repo => repo.Update(It.Is<SubmissionFeedback>(f => f.Id == feedback.Id && f.Declined)))
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(insurerCompany));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(brokerCompany.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(brokerCompany.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();
        notificationService
            .Setup(service => service.SendNotification(It.IsAny<NotificationType>(), It.IsAny<IImmutableList<RecipientDTO>>(), It.IsAny<IImmutableDictionary<string, string>>()))
            .Callback<NotificationType, IImmutableList<RecipientDTO>, IImmutableDictionary<string, string>>((type, recipients, data) =>
            {
                notificationTypeSent = type;
                notifiedRecipients = (ImmutableList<RecipientDTO>)recipients;
                notificationData = (ImmutableDictionary<string, string>)data;
            })
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.DeclineSubmissionCommand(userId, submitDto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(unit => unit.Should().BeAssignableTo(typeof(Shared.Unit)));

        notificationTypeSent.Should().Be(NotificationType.Broker_SubmissionDeclined);
        notifiedRecipients.Select(r => r.Email).Should().BeEquivalentTo(expectedRecipients);
        notificationData["insurer_company"].Should().Be(insurerCompany.Name);
        notificationData["project_name"].Should().Be(deal.Name);
        notificationData["deal_id"].Should().Be(submitDto.SubmissionId.ToString());
    }
}

public class WhenUpdatingTheAssigneesOfADeal : BaseUnitTest
{
    [Test]
    public void GivenTheAssigneesArePartOfTheCompany_ShouldUpdateTheDealAssignees()
    {
        //Arrange
        var company = DataGenerator.Company();
        var userId = company.Employees.First().UserId;
        var existingDeal = DataGenerator.DealSubmissions(companyId: company.Id).First();
        var originalAssignees = new List<EmployeeDTO>()
        {
            Employee.Factory.ToDTO(company.Employees.ToList()[0]),
            Employee.Factory.ToDTO(company.Employees.ToList()[1]),
        };
        var expectedAssignees = new List<EmployeeDTO>()
        {
            Employee.Factory.ToDTO(company.Employees.ToList()[2]),
            Employee.Factory.ToDTO(company.Employees.ToList()[3]),
        };

        var deal = new DealSubmission(
                existingDeal.Id,
                DataGenerator.Fixture.Create<string>(),
                DataGenerator.Fixture.Create<string>(),
                company.Id,
                DataGenerator.Fixture.Create<BasicTerms>(),
                DataGenerator.Fixture.Create<SubmissionPricing>(),
                Enhancement.Factory.Default,
                DataGenerator.Fixture.CreateMany<Warranty>().ToImmutable(),
                originalAssignees.Select(Assignee.Factory.ToEntity).ToImmutable(),
                new List<DealFile>().ToImmutable(),
                new List<FeedbackDetails>().ToImmutable(),
                new List<Modification>().ToImmutable(),
                DateTimeOffset.UtcNow.AddDays(7)
            );
        var assigneeDto = new UpdateDealAssigneesDTO(deal.Id, expectedAssignees.ToImmutable());

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissionDetails(deal.Id))
            .Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(existingDeal)));
        repo.Setup(repo => repo.Update(It.Is<DealSubmission>(d => d.Id == deal.Id)))
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(company));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.UpdateDealAssigneesCommand(userId, assigneeDto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(unit => unit.Should().BeAssignableTo(typeof(Shared.Unit)));
        repo.Verify(repo => repo.Update(It.Is<DealSubmission>(d => AssigneesMatch(d.Assignees.ToList(), expectedAssignees))));
    }
    private bool AssigneesMatch(List<Assignee> actualList, List<EmployeeDTO> expectedList)
    {
        for (int i = 0; i < actualList.Count(); i++)
        {
            var actual = actualList[i];
            var expected = expectedList[i];
            actual.UserId.Value.Should().Be(expected.UserId);
            actual.Name.First.Should().Be(expected.FirstName);
            actual.Name.Last.Should().Be(expected.LastName);
        }
        return true;
    }
}

public class WhenSubmittingASubmission : BaseUnitTest
{
    [Test]
    public void GivenSubmissionSucceeds_ShouldSubmitTheDealAndNotifyInsurers()
    {
        //Arrange
        var notificationTypeSent = NotificationType.None;
        var notifiedRecipients = ImmutableList.Create<RecipientDTO>();
        var notificationData = ImmutableDictionary<string, string>.Empty;
        var insurerCompany1 = DataGenerator.Company(CompanyType.Insurer);
        var insurerCompany2 = DataGenerator.Company(CompanyType.Insurer);
        var expectedRecipients = insurerCompany1.Employees.Select(e => e.Email.Address).Concat(insurerCompany2.Employees.Select(e => e.Email.Address));
        var brokerCompany = DataGenerator.Company();
        var userId = brokerCompany.Employees.First().UserId;
        var deal = DataGenerator.DealSubmissions(brokerCompany.Id).First();
        DealSubmission savedDeal = new EmptySubmission();

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissionDetails(deal.Id))
            .Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(deal)));
        repo.Setup(repo => repo.Update(It.Is<DealSubmission>(d => d.Id == deal.Id)))
            .Callback<DealSubmission>(d => savedDeal = d)
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(brokerCompany));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(insurerCompany1.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(insurerCompany1.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(insurerCompany2.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(insurerCompany2.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();
        notificationService
            .Setup(service => service.SendNotification(It.IsAny<NotificationType>(), It.IsAny<IImmutableList<RecipientDTO>>(), It.IsAny<IImmutableDictionary<string, string>>()))
            .Callback<NotificationType, IImmutableList<RecipientDTO>, IImmutableDictionary<string, string>>((type, recipients, data) =>
            {
                notificationTypeSent = type;
                notifiedRecipients = (ImmutableList<RecipientDTO>)recipients;
                notificationData = (ImmutableDictionary<string, string>)data;
            })
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());

        var submitDto = new SubmitDealDTO(
            deal.Id,
            new List<CompanyDTO> { Company.Factory.ToDTO(insurerCompany1), Company.Factory.ToDTO(insurerCompany2) }.ToImmutable(),
            //DataGenerator.Fixture.CreateMany<MailAddress>().Select(a => a.Address).ToImmutable(),
            DateTimeOffset.UtcNow.AddDays(7)
            );

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.SubmitSumbissionCommand(userId, submitDto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(unit => unit.Should().BeAssignableTo(typeof(Shared.Unit)));

        savedDeal.Feedbacks.Should().HaveCount(2);
        savedDeal.Feedbacks[0].InsuranceCompanyId.Should().Be(insurerCompany1.Id);
        savedDeal.Feedbacks[1].InsuranceCompanyId.Should().Be(insurerCompany2.Id);

        repo.Verify(r => r.Create(It.IsAny<SubmissionFeedback>(), deal), Times.AtLeast(2));

        notificationTypeSent.Should().Be(NotificationType.Insurer_NewSubmission);
        notifiedRecipients.Select(r => r.Email).Should().BeEquivalentTo(expectedRecipients);
        notificationData["broker_company"].Should().Be(brokerCompany.Name);
        notificationData["deal_id"].Should().Be(submitDto.DealId.ToString());

        foreach (var insurerSubmittedTo in submitDto.InsurersToSubmitTo)
        {
            repo.Verify(r => r.Create(It.Is<SubmissionFeedback>(f => f.InsuranceCompanyId == insurerSubmittedTo.Id), It.IsAny<DealSubmission>()));
        }
    }
}

public class WhenModifyingASubmittedSubmission : BaseUnitTest
{
    [Test]
    public void GivenModificationSucceeds_ShouldMarkTheDealForReviewAndNotifyInsurers()
    {
        //Arrange
        var notificationTypeSent = NotificationType.None;
        var notifiedRecipients = ImmutableList.Create<RecipientDTO>();
        var notificationData = ImmutableDictionary<string, string>.Empty;
        var brokerCompany = DataGenerator.Company();
        var userId = brokerCompany.Employees.First().UserId;
        var deal = DataGenerator.DealSubmissions(brokerCompany.Id).First();
        var insurerCompany1 = DataGenerator.Company(CompanyType.Insurer);
        var insurerCompany2 = DataGenerator.Company(CompanyType.Insurer);
        var feedbacks = DataGenerator.SubmissionFeedbacks(deal.Id, new List<Guid> { insurerCompany1.Id, insurerCompany2.Id, Guid.NewGuid() }).ToList();
        var insurerCompany1Feedback = feedbacks[0];
        insurerCompany1Feedback = insurerCompany1Feedback.Submit().ValueOrDefault();
        var insurerCompany2Feedback = feedbacks[1];
        insurerCompany2Feedback = insurerCompany2Feedback.Submit().ValueOrDefault();

        var expectedRecipients = insurerCompany1.Employees.Select(e => e.Email.Address).Concat(insurerCompany2.Employees.Select(e => e.Email.Address));

        deal = deal.Submit(
                        new List<FeedbackDetails> { FeedbackDetails.Factory.Create(insurerCompany1Feedback.Id, insurerCompany1.Id), FeedbackDetails.Factory.Create(insurerCompany2Feedback.Id, insurerCompany2.Id) }.ToImmutable(),
                        DateTimeOffset.Now.AddDays(7))
            .ValueOrDefault();

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissionDetails(deal.Id)).Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(deal)));
        repo.Setup(repo => repo.GetFeedbackDetails(insurerCompany1.Id, deal.Id)).Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(insurerCompany1Feedback)));
        repo.Setup(repo => repo.GetFeedbackDetails(insurerCompany2.Id, deal.Id)).Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(insurerCompany2Feedback)));
        repo.Setup(repo => repo.Update(It.Is<DealSubmission>(d => d.Id == deal.Id))).Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));
        repo.Setup(repo => repo.Update(It.Is<SubmissionFeedback>(f => f.Id == insurerCompany1Feedback.Id && f.ForReview == true))).Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));
        repo.Setup(repo => repo.Update(It.Is<SubmissionFeedback>(f => f.Id == insurerCompany2Feedback.Id && f.ForReview == true))).Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(brokerCompany));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(insurerCompany1.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(insurerCompany1.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(insurerCompany2.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(insurerCompany2.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();
        notificationService
            .Setup(service => service.SendNotification(It.IsAny<NotificationType>(), It.IsAny<IImmutableList<RecipientDTO>>(), It.IsAny<IImmutableDictionary<string, string>>()))
            .Callback<NotificationType, IImmutableList<RecipientDTO>, IImmutableDictionary<string, string>>((type, recipients, data) =>
            {
                notificationTypeSent = type;
                notifiedRecipients = (ImmutableList<RecipientDTO>)recipients;
                notificationData = (ImmutableDictionary<string, string>)data;
            })
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());

        var modifyDto = new ModifySubmittedDealDTO(
            deal.Id,
            DataGenerator.Fixture.Create<string>()
            );

        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        //Act
        var result = SUT.ModifySubmissionCommand(userId, modifyDto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(unit => unit.Should().BeAssignableTo(typeof(Shared.Unit)));
        deal.Modifications.Should().HaveCount(1);
        deal.Modifications.First().Notes.Should().Be(modifyDto.Notes);
        repo.Verify(r => r.Update(It.Is<DealSubmission>(d => d.Id == deal.Id && d.Modifications.Any())));

        notificationTypeSent.Should().Be(NotificationType.Insurer_SubmissionModified);
        notifiedRecipients.Select(r => r.Email).Should().BeEquivalentTo(expectedRecipients);
        notificationData["broker_company"].Should().Be(brokerCompany.Name);
        notificationData["deal_id"].Should().Be(modifyDto.DealId.ToString());
        notificationData["project_name"].Should().Be(deal.Name);
    }
}

public class WhenNudgingAnInsurerForASubmission : BaseUnitTest
{
    private Mock<IDealNotificationService> notificationService;
    private NudgeDTO dto;
    private DealService SUT;
    private UserId userId;
    private Company insurerCompany;
    private SubmissionFeedback feedback;    

    [SetUp]
    public void Setup()
    {
        
        var brokerCompany = DataGenerator.Company();
        userId = brokerCompany.Employees.First().UserId;
        var deal = DataGenerator.DealSubmissions(brokerCompany.Id).First();
        insurerCompany = DataGenerator.Company(CompanyType.Insurer);
        feedback = DataGenerator.SubmissionFeedbacks(deal.Id, new List<Guid> { insurerCompany.Id, Guid.NewGuid(), Guid.NewGuid() }).ToList().First();

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetFeedbackDetails(insurerCompany.Id, deal.Id)).Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(feedback)));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(brokerCompany));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(insurerCompany.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(insurerCompany.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));

        var fileService = new Mock<IDealFileService>();
        notificationService = new Mock<IDealNotificationService>();
        

        dto = new NudgeDTO(
            feedback.Id,
            deal.Id,
            insurerCompany.Id
            );

        SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);
    }

    [Test]
    public void GivenSubmissionFeedbackIsInRightState_ShouldSendNotificationToAllInsuranceComanyEmployees()
    {
        //Arrange
        var notificationTypeSent = NotificationType.None;
        var notifiedRecipients = ImmutableList.Create<RecipientDTO>();
        var notificationData = ImmutableDictionary<string, string>.Empty;

        var expectedRecipients = insurerCompany.Employees.Select(e => e.Email.Address);

        notificationService
            .Setup(service => service.SendNotification(It.IsAny<NotificationType>(), It.IsAny<IImmutableList<RecipientDTO>>(), It.IsAny<IImmutableDictionary<string, string>>()))
            .Callback<NotificationType, IImmutableList<RecipientDTO>, IImmutableDictionary<string, string>>((type, recipients, data) =>
            {
                notificationTypeSent = type;
                notifiedRecipients = (ImmutableList<RecipientDTO>)recipients;
                notificationData = (ImmutableDictionary<string, string>)data;
            })
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());

        

        //Act
        var result = SUT.NudgeInsurerForFeedback(userId, dto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(unit => unit.Should().BeAssignableTo(typeof(Shared.Unit)));

        notificationTypeSent.Should().Be(NotificationType.Insurer_SubmissionFeedbackNudge);
        notifiedRecipients.Select(r => r.Email).Should().BeEquivalentTo(expectedRecipients);
        notificationData["deal_id"].Should().Be(dto.SubmissionId.ToString());
        notificationData["project_name"].Should().Be(feedback.Name);
    }

    [Test]
    public void GivenSubmissionFeedbackIsSubmitted_ShouldNotSendNotification()
    {
        //Arrange
        notificationService
            .Setup(service => service.SendNotification(It.IsAny<NotificationType>(), It.IsAny<IImmutableList<RecipientDTO>>(), It.IsAny<IImmutableDictionary<string, string>>()))
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());
        feedback = feedback.Submit().ValueOrDefault();

        //Act
        var result = SUT.NudgeInsurerForFeedback(userId, dto);

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => 
        {
            error.status.Should().Be(400);
            error.title.Should().Be("Can't send remider for submitted feedback");
        });
    }

    [Test]
    public void GivenSubmissionFeedbackIsDeclined_ShouldNotSendNotification()
    {
        //Arrange
        notificationService
            .Setup(service => service.SendNotification(It.IsAny<NotificationType>(), It.IsAny<IImmutableList<RecipientDTO>>(), It.IsAny<IImmutableDictionary<string, string>>()))
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());
        feedback = feedback.Decline().ValueOrDefault();

        //Act
        var result = SUT.NudgeInsurerForFeedback(userId, dto);

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error =>
        {
            error.status.Should().Be(400);
            error.title.Should().Be("Can't send remider for declined feedback");
        });
    }
}

public class WhenGoingLiveWithASubmission : BaseUnitTest
{
    [Test]
    public void ShouldMarkTheDealAsLiveAndCreateLiveDealAndNotifyInsurers()
    {
        //Arrange
        var notificationTypeSentForLive = NotificationType.None;
        var notifiedRecipientsForLive = ImmutableList.Create<RecipientDTO>();
        var notificationDataForLive = ImmutableDictionary<string, string>.Empty;
        var notificationTypeSentForDeclined = NotificationType.None;
        var notifiedRecipientsForDeclined = ImmutableList.Create<RecipientDTO>();
        var notificationDataForDeclined = ImmutableDictionary<string, string>.Empty;

        var brokerCompany = DataGenerator.Company();
        var userId = brokerCompany.Employees.First().UserId;
        var deal = DataGenerator.DealSubmissions(brokerCompany.Id).First();
        var insurerCompany1 = DataGenerator.Company(CompanyType.Insurer);
        var insurerCompany2 = DataGenerator.Company(CompanyType.Insurer);
        var feedbacks = DataGenerator.SubmissionFeedbacks(deal.Id, new List<Guid> { insurerCompany1.Id, insurerCompany2.Id, Guid.NewGuid() }).ToList();
        var insurerCompany1Feedback = feedbacks[0];
        insurerCompany1Feedback = insurerCompany1Feedback.Submit().ValueOrDefault();
        var insurerCompany2Feedback = feedbacks[1];
        insurerCompany2Feedback = insurerCompany2Feedback.Submit().ValueOrDefault();
        var expectedRecipientsForLive = insurerCompany1.Employees.Select(e => e.Email.Address);
        var expectedRecipientsForDeclined = insurerCompany2.Employees.Select(e => e.Email.Address);

        deal = deal.Submit(
                        new List<FeedbackDetails> { FeedbackDetails.Factory.Create(insurerCompany1Feedback.Id, insurerCompany1.Id), FeedbackDetails.Factory.Create(insurerCompany2Feedback.Id, insurerCompany2.Id) }.ToImmutable(),
                        DateTimeOffset.Now.AddDays(7))
            .ValueOrDefault();

        var repo = new Mock<IDealRepo>();
        repo.Setup(repo => repo.GetSubmissionDetails(deal.Id)).Returns(Task.FromResult(Option.Some<DealSubmission, ErrorCode>(deal)));
        repo.Setup(repo => repo.GetFeedbackDetails(insurerCompany1.Id, deal.Id)).Returns(Task.FromResult(Option.Some<SubmissionFeedback, ErrorCode>(insurerCompany1Feedback)));
        repo.Setup(repo => repo.Update(It.IsAny<DealSubmission>())).Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));
        repo.Setup(repo => repo.Update(It.IsAny<SubmissionFeedback>())).Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));
        repo.Setup(repo => repo.Create(It.IsAny<LiveDeal>())).Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())));

        var companyService = new Mock<ICompanyService>();
        companyService.Setup(service => service.GetCompanyOfUserQuery(userId)).Returns(Option.Some<Company, ErrorCode>(brokerCompany));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(insurerCompany1.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(insurerCompany1.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));
        companyService.Setup(service => service.GetEmployeesForCompanyQuery(insurerCompany2.Id)).Returns(Option.Some<IImmutableList<EmployeeDTO>, ErrorCode>(insurerCompany2.Employees.Select(Employee.Factory.ToDTO).ToImmutable()));

        var fileService = new Mock<IDealFileService>();
        var notificationService = new Mock<IDealNotificationService>();
        int callsCountForSendNotification = 0;
        notificationService
            .Setup(service => service.SendNotification(It.IsAny<NotificationType>(), It.IsAny<IImmutableList<RecipientDTO>>(), It.IsAny<IImmutableDictionary<string, string>>()))
            .Callback<NotificationType, IImmutableList<RecipientDTO>, IImmutableDictionary<string, string>>((type, recipients, data) =>
            {
                if (callsCountForSendNotification++ == 0)//we know we send the success notification first so the first callback will be those values
                {
                    notificationTypeSentForLive = type;
                    notifiedRecipientsForLive = (ImmutableList<RecipientDTO>)recipients;
                    notificationDataForLive = (ImmutableDictionary<string, string>)data;
                }
                else//second callback will be the declined notification
                {
                    notificationTypeSentForDeclined = type;
                    notifiedRecipientsForDeclined = (ImmutableList<RecipientDTO>)recipients;
                    notificationDataForDeclined = (ImmutableDictionary<string, string>)data;
                }                
            })
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());
        
        var SUT = new DealService(repo.Object, companyService.Object, fileService.Object, notificationService.Object);

        var dto = new GoLiveDTO(insurerCompany1Feedback.Id, deal.Id, insurerCompany1.Id);

        //Act
        var result = SUT.GoLiveCommand(userId, dto);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(unit => unit.Should().BeAssignableTo(typeof(Shared.Unit)));

        repo.Verify(repo => repo.Update(It.Is<DealSubmission>(d => d.Id == deal.Id)), Times.Once);
        repo.Verify(repo => repo.Update(It.Is<SubmissionFeedback>(f => f.Id == insurerCompany1Feedback.Id && f.IsLive == true)), Times.Once);
        repo.Verify(
            repo => repo.Create(
                It.Is<LiveDeal>(d => 
                    //d.SubmissionId == deal.Id &&
                    d.FeedbackId == insurerCompany1Feedback.Id &&
                    d.BrokerCompanyId == brokerCompany.Id &&
                    d.InsuranceCompanyId == insurerCompany1.Id
                    )
                ),
            Times.Once);

        notificationTypeSentForLive.Should().Be(NotificationType.Insurer_SubmissionFeedbackAccepted);
        notifiedRecipientsForLive.Select(r => r.Email).Should().BeEquivalentTo(expectedRecipientsForLive);
        notificationDataForLive["deal_id"].Should().Be(dto.SubmissionId.ToString());
        notificationDataForLive["project_name"].Should().Be(deal.Name);

        notificationTypeSentForDeclined.Should().Be(NotificationType.Insurer_SubmissionFeedbackDeclined);
        notifiedRecipientsForDeclined.Select(r => r.Email).Should().BeEquivalentTo(expectedRecipientsForDeclined);
        notificationDataForDeclined["deal_id"].Should().Be(dto.SubmissionId.ToString());
        notificationDataForDeclined["project_name"].Should().Be(deal.Name);
    }
}