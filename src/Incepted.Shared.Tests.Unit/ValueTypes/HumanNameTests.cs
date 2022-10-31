using AutoFixture;
using FluentAssertions;
using Incepted.Shared.Tests.Unit.DataSeeding;
using Incepted.Shared.ValueTypes;
using NUnit.Framework;
using System;
using System.Linq;

namespace Incepted.Shared.Tests.Unit.ValueTypes;

internal class HumanNameTests
{
    public class WhenCreatingAnEmployee
    {
        [Test]
        public void GivenTheDetailsAreCorrect_ShouldReturnANewHumanName()
        {
            //Arrange
            var expectedFirstName = Guid.NewGuid().ToString();
            var expectedLastName = Guid.NewGuid().ToString();

            //Act
            var result = new HumanName(expectedFirstName, expectedLastName);

            //Assert
            result.First.Should().Be(expectedFirstName);
            result.Last.Should().BeEquivalentTo(expectedLastName);
        }

        [TestCase(null)]
        [TestCase("")]
        public void GivenNameIsEmpty_ShouldThrowArgumentException(string name)
        {
            //Arrange

            //Act
            var action = () => new HumanName(name, "name");

            //Assert
            action.Should().Throw<ArgumentException>().WithMessage("First name can't be empty (Parameter 'firstName')");

            //Act
            action = () => new HumanName("name", name);

            //Assert
            action.Should().Throw<ArgumentException>().WithMessage("Last name can't be empty (Parameter 'lastName')");
        }
    }

    public class WhenUpdatingAName
    {
        [Test]
        public void GivenValidValues_ShouldReturnSomeUpdatedHumanName()
        {
            //Arrange
            var SUT = DataGenerator.Fixture.Create<HumanName>();
            var newFirst = DataGenerator.Fixture.Create<string>();
            var newLast = DataGenerator.Fixture.Create<string>();

            //Act
            var result = SUT.Update(newFirst, newLast);

            //Assert
            result.MatchSome(updatedName => {
                updatedName.First.Should().Be(newFirst);
                updatedName.Last.Should().Be(newLast);
            });
        }

        [TestCase(null)]
        [TestCase("")]
        public void GivenFirstOrLastNameIsEmpty_ShouldReturnNoneWithError(string name)
        {
            //Arrange
            var SUT = DataGenerator.Fixture.Create<HumanName>();
            
            //Act
            var result = SUT.Update(name, "last");

            //Assert
            result.MatchNone(error => error.errors.name.First().Should().Be("The first name can't be empty."));

            //Act
            result = SUT.Update("first", name);

            //Assert
            result.MatchNone(error => error.errors.name.First().Should().Be("The last name can't be empty."));
        }
    }
}
