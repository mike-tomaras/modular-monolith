using AutoFixture;
using FluentAssertions;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using NUnit.Framework;

namespace Incepted.Shared.Tests.Unit.DTOs;

public class CompanyDTOTests
{
    [Test]
    public void ShouldGetCompanyName()
    {
        //Arrange
        var SUT = DataGenerator.Fixture.Create<CompanyDTO>();

        //Act

        //Assert
        SUT.ToString().Should().Be(SUT.Name);
    }
}