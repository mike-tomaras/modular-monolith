using FluentAssertions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;
using System.Net.Mail;

namespace Incepted.Domain.Deals.Tests.Unit.DomainTests;

public class WhenCreatingAnAssignee : BaseUnitTest
{
    [Test]
    public void GivenTheDetailsAreCorrect_ShouldReturnANewAssignee()
    {
        //Arrange
        var expectedAssignee = DataGenerator.Assignee();


        //Act
        var result = new Assignee(expectedAssignee.Id, expectedAssignee.UserId, expectedAssignee.Name, expectedAssignee.Email);

        //Assert
        result.UserId.Should().BeEquivalentTo(expectedAssignee.UserId);
    }

    [Test]
    public void GivenIdIsNull_ShouldThrowArgumentException()
    {
        //Arrange

        //Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var action = () => new Assignee(Guid.NewGuid(), null, new HumanName("name", "name"), new MailAddress("email@email.com"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void GivenNameIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange

        //Act
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var action = () => new Assignee(Guid.NewGuid(), new UserId("auth0|123"), null, new MailAddress("email@email.com"));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        //Assert
        action.Should().Throw<ArgumentNullException>();
    }
}
public class WhenGettingAnAssigneeFromAnAssigneeDTO : BaseUnitTest
{
    [Test]
    public void ShouldReturnTheDTO()
    {
        //Arrange
        var dto = DataGenerator.EmployeeDTO();

        //Act
        var result = Assignee.Factory.ToEntity(dto);

        //Assert
        result.UserId.Value.Should().Be(dto.UserId);
        result.Name.First.Should().Be(dto.FirstName);
        result.Name.Last.Should().Be(dto.LastName);
    }
}
