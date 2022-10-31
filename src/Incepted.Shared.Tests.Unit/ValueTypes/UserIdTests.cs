using FluentAssertions;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;

namespace Incepted.Shared.Tests.Unit.ValueTypes;

public class WhenCreatingAUserId
{
    [Test]
    public void GivenAValidUserId_ShouldReturnSomeObject()
    {
        //Arrange
        var userIdString = "auth0|626e80401d742f006f2ab6a6";

        //Act
        var result = new UserId(userIdString);

        //Assert
        result.Value.Should().Be(userIdString);
        result.ToString().Should().Be(userIdString);
    }

    [TestCase("abc|626e80401d742f006f2ab6a6")]//wrong id type
    [TestCase("auth0626e80401d742f006f2ab6a6")]//id type and id are not separated by '|'
    [TestCase("626e80401d742f006f2ab6a6")]//no id type
    public void GivenAnInvalidUserId_ShouldReturnNone(string userIdString)
    {
        //Arrange

        //Act
        Action action = () => new UserId(userIdString);

        //Assert
        action.Should().Throw<ArgumentException>().WithMessage("UserId string is not in the correct format. (Parameter 'userIdString')");
    }
}