using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Companies.Application;
using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Application;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using Moq;
using NUnit.Framework;
using Optional;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Incepted.Domain.Companies.Tests.Unit;

public class WhenGettingAllTheCompaniesOfACertainType : BaseUnitTest
{
    [Test]
    public void ShouldReturnAllTheCompaniesOfTheSpecificType()
    {
        //Arrange
        var brokerCompany = DataGenerator.Company(CompanyType.Broker);
        var insurerCompany = DataGenerator.Company(CompanyType.Insurer);
        
        var repo = new Mock<ICompanyRepo>();
        repo.Setup(repo => repo.GetCompaniesOfType(CompanyType.Broker))
            .Returns(Task.FromResult(Option.Some<IImmutableList<Company>, ErrorCode>(ImmutableList.Create(brokerCompany))));
        repo.Setup(repo => repo.GetCompaniesOfType(CompanyType.Insurer))
            .Returns(Task.FromResult(Option.Some<IImmutableList<Company>, ErrorCode>(ImmutableList.Create(insurerCompany))));

        var SUT = new CompanyService(repo.Object, new Mock<ICompanyNotificationService>().Object, new Mock<ICompanyFileService>().Object);

        //Act
        var result = SUT.GetCompaniesOfType(CompanyType.Broker);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(companies =>
            companies.First().Id.Should().Be(brokerCompany.Id)
        );
    }
}

public class WhenGettingTheCompanyOfAUser : BaseUnitTest
{
    [Test]
    public void GivenTheUserIdIsPartOfACompany_ShouldReturnTheDefaultAssignees()
    {
        //Arrange
        var expectedCompany = DataGenerator.Company();
        var employee = expectedCompany.Employees.First();
        
        var repo = new Mock<ICompanyRepo>();
        repo.Setup(repo => repo.GetCompanyOfEmployee(employee.UserId)).Returns(Task.FromResult(Option.Some<Company, ErrorCode>(expectedCompany)));
        
        var SUT = new CompanyService(repo.Object, new Mock<ICompanyNotificationService>().Object, new Mock<ICompanyFileService>().Object);

        //Act
        var result = SUT.GetCompanyOfUserQuery(employee.UserId);

        //Assert
        var company = result.ValueOr(() => throw new Exception("result should have a value"));
        company.Should().NotBeNull();
        company.Id.Should().Be(expectedCompany.Id);
    }
}

public class WhenGettingTheEmployeesOfACompanyByEmployeeId : BaseUnitTest
{
    [Test]
    public void GivenTheUserIdIsPartOfACompany_ShouldReturnTheEmployees()
    {
        //Arrange
        var expectedCompany = DataGenerator.Company();
        var employee = expectedCompany.Employees.First();
        
        var repo = new Mock<ICompanyRepo>();
        repo.Setup(repo => repo.GetCompanyOfEmployee(employee.UserId)).Returns(Task.FromResult(Option.Some<Company, ErrorCode>(expectedCompany)));
        
        var SUT = new CompanyService(repo.Object, new Mock<ICompanyNotificationService>().Object, new Mock<ICompanyFileService>().Object);

        //Act
        var result = SUT.GetEmployeesForCompanyOfUserQuery(employee.UserId);

        //Assert
        var employees = result.ValueOr(ImmutableList.Create<EmployeeDTO>());
        employees.Should()
            .AllSatisfy(employee => { 
                var expectedEmployee = expectedCompany.Employees.SingleOrDefault(e => e.UserId.Value == employee.UserId);
                expectedEmployee.Should().NotBeNull();
                employee.FirstName.Should().Be(expectedEmployee?.Name.First);
                employee.LastName.Should().Be(expectedEmployee?.Name.Last);
            });
    }
}

public class WhenGettingTheEmployeesOfACompanyByCompanyId : BaseUnitTest
{
    [Test]
    public void GivenCompanyExists_ShouldReturnTheCompany()
    {
        //Arrange
        var expectedCompany = DataGenerator.Company();
        
        var repo = new Mock<ICompanyRepo>();
        repo.Setup(repo => repo.GetCompany(expectedCompany.Id)).Returns(Task.FromResult(Option.Some<Company, ErrorCode>(expectedCompany)));

        var SUT = new CompanyService(repo.Object, new Mock<ICompanyNotificationService>().Object, new Mock<ICompanyFileService>().Object);

        //Act
        var result = SUT.GetEmployeesForCompanyQuery(expectedCompany.Id);

        //Assert
        var employees = result.ValueOr(ImmutableList.Create<EmployeeDTO>());
        employees.Should()
            .AllSatisfy(employee => {
                var expectedEmployee = expectedCompany.Employees.SingleOrDefault(e => e.UserId.Value == employee.UserId);
                expectedEmployee.Should().NotBeNull();
                employee.FirstName.Should().Be(expectedEmployee?.Name.First);
                employee.LastName.Should().Be(expectedEmployee?.Name.Last);
            });
    }
}

public class WhenUpdatingAnEmployeeOfACompany : BaseUnitTest
{
    [Test]
    public void GivenCompanyAndEmployeeExistAndDataIsValid_ShouldReturnTheCompany()
    {
        //Arrange
        var company = DataGenerator.Company();
        var userId = company.Employees.First().UserId;
        var userDTO = new UserDTO("first", "last", "valid@email.com");
        Employee? updatedEmployee = null;
        UserId? notificationUserId = null;
        UserDTO? notificationData = null;

    var repo = new Mock<ICompanyRepo>();
        repo.Setup(repo => repo.GetCompanyOfEmployee(userId)).Returns(Task.FromResult(Option.Some<Company, ErrorCode>(company)));
        repo.Setup(repo => repo.Update(It.IsAny<Employee>(), company.Id))
            .Returns(Task.FromResult(Option.Some<Shared.Unit, ErrorCode>(new Shared.Unit())))
            .Callback<Employee, Guid>((e, id) => updatedEmployee = e);

        var notificationService = new Mock<ICompanyNotificationService>();
        notificationService
            .Setup(service => service.NotifyAdminOfUserDetailsChangeRequest(It.IsAny<UserId>(), It.IsAny<UserDTO>()))
            .Callback<UserId, UserDTO>((id, dto) => {
                notificationUserId = id;
                notificationData = dto;
            })
            .Returns(new Shared.Unit().Some<Shared.Unit, ErrorCode>());

        var SUT = new CompanyService(repo.Object, notificationService.Object, new Mock<ICompanyFileService>().Object);

        //Act
        var result = SUT.UpdateEmployeeOfCompanyQuery(userId, userDTO);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(unit => unit.Should().BeAssignableTo(typeof(Shared.Unit)));
        
        updatedEmployee.Should().NotBeNull();
        updatedEmployee.Name.First.Should().Be(userDTO.FirstName);
        updatedEmployee.Name.Last.Should().Be(userDTO.LastName);
        updatedEmployee.Email.Should().Be(userDTO.Email);

        notificationUserId.Should().BeEquivalentTo(userId);
        notificationData.Should().BeEquivalentTo(userDTO);
    }
}

public class WhenGettingTheDefaultTCsOfAnInsuranceCompany : BaseUnitTest
{
    [Test]
    public void GivenTheUserIdIsPartOfACompany_ShouldReturnTheFile()
    {
        //Arrange
        var expectedCompany = DataGenerator.Company();
        var employee = expectedCompany.Employees.First();

        var repo = new Mock<ICompanyRepo>();
        repo.Setup(repo => repo.GetCompanyOfEmployee(employee.UserId)).Returns(Task.FromResult(Option.Some<Company, ErrorCode>(expectedCompany)));

        var fileService = new Mock<ICompanyFileService>();

        var SUT = new CompanyService(repo.Object, new Mock<ICompanyNotificationService>().Object, fileService.Object);

        //Act
        var result = SUT.GetDefaultTsAndCs(employee.UserId);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(file => file.Should().BeEquivalentTo(CompanyFile.Factory.ToDTO(expectedCompany.TsAndCs)));
    }
}

public class WhenSettingTheDefaultTCsOfAnInsuranceCompany : BaseUnitTest
{
    [Test]
    public void GivenTheUserIdIsPartOfACompany_ShouldUploadTheFile()
    {
        //Arrange
        var expectedCompany = DataGenerator.Company();
        var employee = expectedCompany.Employees.First();
        var expectedFile = new CompanyFile(Guid.NewGuid(), Guid.NewGuid().ToString() + ".pdf", expectedCompany.TsAndCs.StoredFileName, FileType.InsurerTCs, DateTimeOffset.Now.AddDays(-3));
        var expectedFileDTO = CompanyFile.Factory.ToDTO(expectedFile);
        var expectedFileUploadResult = new FileUploadResult(expectedFileDTO, true, DataGenerator.Fixture.Create<ErrorCode>());
        Company savedCompany = new EmptyCompany();

        var repo = new Mock<ICompanyRepo>();
        repo.Setup(repo => repo.GetCompanyOfEmployee(employee.UserId)).Returns(Task.FromResult(Option.Some<Company, ErrorCode>(expectedCompany)));
        repo.Setup(repo => repo.Update(It.IsAny<Company>())).Callback<Company>((company) => savedCompany = company).Returns(Task.FromResult(new Shared.Unit().Some<Shared.Unit, ErrorCode>()));

        var fileService = new Mock<ICompanyFileService>();
        fileService.Setup(service => service.UploadAsync(expectedCompany.Id, expectedFileDTO, It.IsAny<Stream>())).Returns(Task.FromResult(expectedFileUploadResult));

        var SUT = new CompanyService(repo.Object, new Mock<ICompanyNotificationService>().Object, fileService.Object);

        //Act
        var result = SUT.SetDefaultTsAndCs(employee.UserId, expectedFileDTO, new MemoryStream());

        //Assert
        result.HasValue.Should().BeTrue();
        savedCompany.TsAndCs.Should().NotBeNull();
        savedCompany.TsAndCs.Should().BeEquivalentTo(expectedFile);
    }
}

