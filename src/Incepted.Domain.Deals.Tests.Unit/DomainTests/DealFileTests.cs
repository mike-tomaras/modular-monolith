using AutoFixture;
using FluentAssertions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.Enums;
using Incepted.Shared.Tests.Unit.DataSeeding;
using NUnit.Framework;
using System;

namespace Incepted.Domain.Deals.Tests.Unit.DomainTests;

public class WhenCreatingADealFile : BaseUnitTest
{
    [Test]
    public void GivenCorrectData_ShouldCreateADealFile()
    {
        //Arrange
        var expectedId = Guid.NewGuid();
        var expectedName = $"{DataGenerator.Fixture.Create<string>()}.pdf";
        var expectedStoredName = DataGenerator.Fixture.Create<string>();
        var expectedLastModified = DateTimeOffset.Now.AddDays(-1);
        var expectedType = FileType.None;
        var expectedContentType = "application/pdf";

        //Act
        var result = new DealFile(expectedId, expectedName, expectedStoredName, expectedType, expectedLastModified);

        //Assert
        result.Id.Should().Be(expectedId);
        result.FileName.Should().Be(expectedName);
        result.StoredFileName.Should().Be(expectedStoredName);
        result.Type.Should().Be(expectedType);
        result.LastModified.Should().Be(expectedLastModified);
        result.ContentType.ToString().Should().Be(expectedContentType);
    }

    [Test]
    public void GivenIdIsEmpty_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new DealFile(Guid.Empty, "name.pdf", "", FileType.SPA, DateTimeOffset.Now);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Deal file Id can't be empty (Parameter 'DealFile id')");
    }
    [TestCase(null)]
    [TestCase("")]
    public void GivenNameIsEmpty_ShouldThrowArgumentException(string name)
    {
        //Arrange


        //Act
        var action = () => new DealFile(Guid.NewGuid(), name, "", FileType.SPA, DateTimeOffset.Now);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Deal file name can't be empty (Parameter 'DealFile fileName')");
    }

    [Test]
    public void GivenlastModifiedIsInTheFuture_ShouldThrowArgumentException()
    {
        //Arrange


        //Act
        var action = () => new DealFile(Guid.NewGuid(), "name.pdf", "", FileType.SPA, DateTimeOffset.Now.AddHours(1));

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("Deal file last modified date can't be in the future (Parameter 'DealFile lastModified')");
    }
}
