using AutoFixture;
using FluentAssertions;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Incepted.Shared.Tests.Unit.DTOs;

public class AssigneeDTOTests
{
    [Test]
    public void ShouldGetAssigneeInitials()
    {
        //Arrange
        var SUT = new EmployeeDTO(Guid.NewGuid(), Guid.NewGuid().ToString(), "Abbot", "Costello", "email@email.com");

        //Act

        //Assert
        SUT.Initials.Should().Be("AC");
    }

    [Test]
    public void ShouldGetAssigneeFullName()
    {
        //Arrange
        var SUT = new EmployeeDTO(Guid.NewGuid(), Guid.NewGuid().ToString(), "Abbot", "Costello", "email@email.com");

        //Act

        //Assert
        SUT.FullName.Should().Be("Abbot Costello");
    }
}

public class UpdateAssigneeDTOTests
{
    [Test]
    public void ShouldGetAssigneeIds()
    {
        //Arrange
        var SUT = new UpdateDealAssigneesDTO(
            Guid.NewGuid(),
            ImmutableList.CreateRange(
                new List<EmployeeDTO> 
                { 
                    new EmployeeDTO(Guid.NewGuid(), DataGenerator.Fixture.Create<UserId>().Value, DataGenerator.Fixture.Create<string>(), "email@email.com", DataGenerator.Fixture.Create<string>()),
                    new EmployeeDTO(Guid.NewGuid(), DataGenerator.Fixture.Create<UserId>().Value, DataGenerator.Fixture.Create<string>(), "email@email.com", DataGenerator.Fixture.Create<string>()),
                    new EmployeeDTO(Guid.NewGuid(), DataGenerator.Fixture.Create<UserId>().Value, DataGenerator.Fixture.Create<string>(), "email@email.com", DataGenerator.Fixture.Create<string>()),
                })
            );
        var assignees = SUT.Assignees.ToImmutable();
        var expectedIds = new List<UserId> { new UserId(assignees[0].UserId), new UserId(assignees[1].UserId), new UserId(assignees[2].UserId) };

        //Act

        //Assert
        SUT.AssigneeIds.Should().BeEquivalentTo(expectedIds);
    }
}