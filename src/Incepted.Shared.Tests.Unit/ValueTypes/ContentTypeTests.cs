using FluentAssertions;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;

namespace Incepted.Shared.Tests.Unit.ValueTypes;

public class ContentTypeTests
{
    [Test]
    public void WhenCreatingAContentTypeObj_GivenValidParams_ShouldReturnTheNewContentTypeObj()
    {
        //Arrange
        var expected = "application/pdf";
        var fileExtension = ".pdf";

        //Act
        var result = new ContentType(fileExtension);

        //Assert
        result.ToString().Should().Be(expected);
    }

    [TestCase("")]
    [TestCase(null)]
    public void WhenCreatingAContentTypeObj_GivenNoFileExtention_ShouldThrow(string fileExtension)
    {
        //Arrange

        //Act
        var action = () => new ContentType(fileExtension);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("File extension can't be empty when getting content type (Parameter 'fileExtension')");
    }

    [Test]
    public void WhenCreatingAContentTypeObj_GivenUnmappedExtention_ShouldThrow()
    {
        //Arrange
        var fileExtension = ".isnotmapped";

        //Act
        var action = () => new ContentType(fileExtension);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("File extension is not mapped to a valid content type (Parameter 'fileExtension')");
    }
}