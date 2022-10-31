using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Companies.Entities;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Incepted.Domain.Companies.Tests.Unit;

public class WhenCreatingACompany : BaseUnitTest
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewCompany()
    {
        //Arrange
        var expectedCompany = DataGenerator.Company();
        
        //Act
        var result = new Company(expectedCompany.Id, expectedCompany.Name, expectedCompany.Type, expectedCompany.Employees, expectedCompany.TsAndCs);

        //Assert
        result.Should().BeEquivalentTo(expectedCompany);
    }

    [Test]        
    public void GivenIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange
        

        //Act
        var action = () => new Company(Guid.Empty, "name", CompanyType.Broker, ImmutableList.Create<Employee>(), new EmptyFile());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Company Id can't be empty (Parameter 'Company id')");
    }

    [TestCase(null)]
    [TestCase("")]
    public void GivenNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new Company(Guid.NewGuid(), name, CompanyType.Broker, ImmutableList.Create<Employee>(), new EmptyFile());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Company name can't be empty (Parameter 'Company name')");
    }

    [Test]
    public void GivenNoEmployees_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new Company(Guid.NewGuid(), "name", CompanyType.Broker, ImmutableList.Create<Employee>(), new EmptyFile());

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Company must have at least one employee (Parameter 'Company employees')");
    }
}

public class WhenValidatingAssigneesAgainstEmployees : BaseUnitTest
{
    private List<EmployeeDTO> assignees;

    public Company SUT { get; private set; }

    [SetUp]
    public void Setup()
    {
        var company = DataGenerator.Company();
        var employee = company.Employees.First();

        assignees = company.Employees.Select(Employee.Factory.ToDTO).ToList();

        SUT = company;
    }

    [Test]
    public void GivenAllAssigneesAreValidEmployees_ShouldReturnTrue()
    {
        //Arrange
        

        //Act
        var result = SUT.AreAssigneesValidEmployees(assignees.Select(a => new UserId(a.UserId)));

        //Assert
        result.Should().BeTrue();
    }

    [Test]
    public void GivenAnyAssigneeDoesNotMatchAnEmployee_ShouldReturnFalse()
    {
        //Arrange
        assignees.Add(new EmployeeDTO(Guid.NewGuid(), "auth0|someid", "first", "last", "email@email.com"));
        
        //Act
        var result = SUT.AreAssigneesValidEmployees(assignees.Select(a => new UserId(a.UserId)));

        //Assert
        result.Should().BeFalse();
    }
}

public class WhenSettingTheDefaultTsAndCs : BaseUnitTest
{

    [Test]
    public void GivenNoTsAndCs_ShouldKeepTheSameStoredName()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Company>();
        SUT = new Company(SUT.Id, SUT.Name, CompanyType.Insurer, SUT.Employees, new EmptyFile());
        var newFile = DataGenerator.Fixture.Create<CompanyFile>();

        //Act
        var result = SUT.SetTcAndCs(newFile);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(company => {
            company.Should().NotBeSameAs(SUT);
            company.TsAndCs.Should().Be(newFile);
        }
        );
    }
}

public class WhenCreatingACompanyDTO : BaseUnitTest
{
    [Test]
    public void GivenAllAssigneesAreValidEmployees_ShouldReturnTrue()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<Company>();

        //Act
        var result = Company.Factory.ToDTO(SUT);

        //Assert
        result.Id.Should().Be(SUT.Id);
        result.Name.Should().Be(SUT.Name);
    }
}

