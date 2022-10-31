using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Companies.Entities;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net.Mail;

namespace Incepted.Domain.Companies.Tests.Unit;

public class WhenCreatingAnEmployee : BaseUnitTest
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewEmployee()
    {
        //Arrange
        var expectedEmployee = DataGenerator.Employee();

        //Act
        var result = new Employee(expectedEmployee.Id, expectedEmployee.UserId, expectedEmployee.Name, expectedEmployee.Email);

        //Assert
        result.UserId.Should().BeEquivalentTo(expectedEmployee.UserId);
        result.Name.Should().BeEquivalentTo(expectedEmployee.Name);
        result.Email.Address.Should().Be(expectedEmployee.Email.Address);
    }

    [Test]
    public void GivenIdIsNull_ShouldThrowArgumentException()
    {
        //Arrange

        //Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var action = () => new Employee(Guid.NewGuid(), null, new HumanName("name", "name"), new MailAddress("test@test.com"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }
    [TestCase(null)]
    [TestCase("")]
    public void GivenNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange

        //Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var action = () => new Employee(Guid.NewGuid(), new UserId("auth0|123"), null, new MailAddress("test@test.com"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }
}
public class WhenGettingAnAssigneeFromAnEmployee : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDTO()
    {
        //Arrange
        var SUT = DataGenerator.Employee();

        //Act
        var result = Employee.Factory.ToDTO(SUT);

        //Assert
        result.UserId.Should().BeEquivalentTo(SUT.UserId.Value);
        result.FirstName.Should().BeEquivalentTo(SUT.Name.First);
        result.LastName.Should().BeEquivalentTo(SUT.Name.Last);
    }
}

public class WhenUpdatingTheDetailsOfAnEmployee : BaseUnitTest
{
    private Employee SUT;
    private UserDTO updateDTO;

    [SetUp]
    public void Setup()
    {
        SUT = DataGenerator.Fixture.Create<Employee>();
        updateDTO = new UserDTO(DataGenerator.Fixture.Create<string>(), DataGenerator.Fixture.Create<string>(), $"{DataGenerator.Fixture.Create<string>()}@email.com");
    }

    [Test]
    public void GivenTheDetailsAreValid_ShouldReturnSomeEmployee()
    {
        //Arrange
        

        //Act
        var result = SUT.Update(updateDTO);

        //Assert
        result.HasValue.Should().BeTrue();
        result.MatchSome(updatedEmployee => {
            //update these
            updatedEmployee.Name.ToString().Should().Be(new HumanName(updateDTO.FirstName, updateDTO.LastName).ToString());
            updatedEmployee.Email.ToString().Should().Be(updateDTO.Email);
            //these should not be updated
            updatedEmployee.UserId.ToString().Should().Be(SUT.UserId.ToString());
        });
    }

    [Test]
    public void GivenTheEmailIsInvalid_ShouldReturnNoneWithError()
    {
        //Arrange
        updateDTO = updateDTO with { Email = "invalid" };

        //Act
        var result = SUT.Update(updateDTO);

        //Assert
        result.HasValue.Should().BeFalse();
        result.MatchNone(error => error.errors.name.First().Should().Be("The email is not valid.") );
    }
}
