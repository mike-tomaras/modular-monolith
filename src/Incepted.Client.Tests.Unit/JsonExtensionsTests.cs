using AutoFixture;
using FluentAssertions;
using Incepted.Client.Extensions;
using NUnit.Framework;
using Optional;
using System;
using System.Text.Json;

namespace Incepted.Client.Tests.Unit;

public class WhenDeserializingJson : BaseUnitTest
{
    [Test]
    public void GivenAValidJson_ShouldReturnSomeObject()
    {
        //Arrange            
        var initialObject = DataGenerator.Create<TestObject>();
        var json = JsonSerializer.Serialize(initialObject);

        //Act
        var result = JsonExtensions.DeserializeJson<TestObject>(json);

        //Assert
        result.MatchSome(x => x.Should().Be(initialObject));
    }


    [Test]
    public void GivenAnInvalidJson_ShouldReturnNone()
    {
        //Arrange            
        var json = "#Bad json! {}"; 

        //Act
        var result = JsonExtensions.DeserializeJson<TestObject>(json);

        //Assert
        result.Should().Be(Option.None<TestObject>());
    }

    [Test]
    public void GivenAValidButEmptyJson_ShouldReturnNone()
    {
        //Arrange            
        string json = "{}";

        //Act
        var result = JsonExtensions.DeserializeJson<TestObject>(json);

        //Assert
        result.MatchSome(x => x.Should().Be(new TestObject(default(double), default(int), default(string), default(Guid))));
    }

    [Test]
    public void GivenANullInput_ShouldReturnNone()
    {
        //Arrange            
        string? json = null;

        //Act
#pragma warning disable CS8604 // Possible null reference argument.
        var result = JsonExtensions.DeserializeJson<TestObject>(json);
#pragma warning restore CS8604 // Possible null reference argument.

        //Assert
        result.Should().Be(Option.None<TestObject>());
    }

    public record TestObject(double dblVal, int intVal, string? strval, Guid guidVal);
}
