using FluentAssertions;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Incepted.Shared.Tests.Unit.DTOs;

public class DealDTOTests
{
    [Test]
    public void ShouldGetAssigneesString()
    {
        //Arrange
        var companyId = Guid.NewGuid();
        var SUT = new DealListItemDTO(
            Guid.NewGuid(),
            "Some Deal",
            "Some Broker",
            "Energy",
            "UK",
            false,
            new Money(123456, Currency.GBP),
            ImmutableList.CreateRange(new List<EmployeeDTO>
            {
                new EmployeeDTO(Guid.NewGuid(), Guid.NewGuid().ToString(), "First1", "Last1", "email@email.com"),
                new EmployeeDTO(Guid.NewGuid(), Guid.NewGuid().ToString(), "First2", "Last2", "email@email.com"),
                new EmployeeDTO(Guid.NewGuid(), Guid.NewGuid().ToString(), "First3", "Last3", "email@email.com"),
            }),
            DateTimeOffset.UtcNow.AddDays(7));

        //Act

        //Assert
        SUT.AssigneesString.Should().Be("Assignees: First1 Last1 - First2 Last2 - First3 Last3");
    }

    [Test]
    public void ShouldGetEmptyObjects()
    {
        //Arrange

        //Act
        var SUT1 = DealSubmissionDTO.Factory.Empty;

        //Assert
        SUT1.Id.Should().Be(Guid.Empty);
        SUT1.Name.Should().BeEmpty();
        SUT1.Pricing.EnterpriseValue.Amount.Should().Be(0);
        SUT1.Assignees.Should().BeEmpty();
        SUT1.Files.Should().BeEmpty();

        //Act
        var SUT2 = DealSubmissionDTO.Factory.Empty;

        //Assert
        SUT2.Id.Should().Be(Guid.Empty);
        SUT2.Name.Should().BeEmpty();
        SUT2.Pricing.EnterpriseValue.Amount.Should().Be(0);
        SUT2.Assignees.Should().BeEmpty();
        SUT2.Files.Should().BeEmpty();
    }
}