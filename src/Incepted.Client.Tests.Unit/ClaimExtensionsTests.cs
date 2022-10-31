using AutoFixture;
using FluentAssertions;
using Incepted.Client.Extensions;
using Incepted.Shared.DTOs;
using NUnit.Framework;
using Optional;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace Incepted.Client.Tests.Unit
{
    public class WhenGettingAClaimValue : BaseUnitTest
    {
        private List<Claim> _claims = new List<Claim>();

        [SetUp]
        public void Setup()
        {
            _claims = Enumerable.Range(0, 3)
                .Select(i => new Claim(DataGenerator.Create<string>(), DataGenerator.Create<string>()))
                .ToList();
        }

        [Test]
        public void GivenAnExistingClaimType_ShouldReturnSomeClaimValue()
        {
            //Arrange            
            var claim = _claims.First();

            //Act
            var result = _claims.GetClaimValue(claim.Type);

            //Assert
            result.Should().Be(Option.Some(claim.Value));
        }


        [Test]
        public void GivenAnNonExistingClaimType_ShouldReturnNone()
        {
            //Arrange
            var claim = new Claim(DataGenerator.Create<string>(), DataGenerator.Create<string>());

            //Act
            var result = _claims.GetClaimValue(claim.Type);

            //Assert
            result.Should().Be(Option.None<string>());
        }

        [Test]
        public void GivenAnEmptyArrayClaimValue_ShouldReturnNone()
        {
            //Arrange
            var claim = new Claim(DataGenerator.Create<string>(), "[]");

            //Act
            var result = _claims.GetClaimValue(claim.Type);

            //Assert
            result.Should().Be(Option.None<string>());
        }
    }

    public class WhenGettingTheUserIdFromAClaim : BaseUnitTest
    {
        private List<Claim> _claims = new List<Claim>();

        [SetUp]
        public void Setup()
        {
            _claims = Enumerable.Range(0, 3)
                .Select(i => new Claim(DataGenerator.Create<string>(), DataGenerator.Create<string>()))
                .ToList();
        }

        [Test]
        public void GivenAnExistingPictureClaim_ShouldReturnSomeAvatar()
        {
            //Arrange            
            var id = DataGenerator.Create<string>();
            _claims.Add(new Claim("sub", id));

            //Act
            var result = _claims.GetUserId();

            //Assert
            result.Should().Be(Option.Some(id));
        }


        [Test]
        public void GivenAnNonExistingPictureClaim_ShouldReturnDefaultAvatar()
        {
            //Arrange

            //Act
            var result = _claims.GetUserId();

            //Assert
            result.Should().Be(Option.None<string>());
        }
    }

    public class WhenGettingTheAvatarFromAClaim : BaseUnitTest
    {
        private List<Claim> _claims = new List<Claim>();
        
        [SetUp]
        public void Setup()
        {
            _claims = Enumerable.Range(0, 3)
                .Select(i => new Claim(DataGenerator.Create<string>(), DataGenerator.Create<string>()))
                .ToList();            
        }

        [Test]
        public void GivenAnExistingPictureClaim_ShouldReturnSomeAvatar()
        {
            //Arrange            
            var avatar = "avatarlink";
            _claims.Add(new Claim("picture", avatar));

            //Act
            var result = _claims.GetAvatar();

            //Assert
            result.Should().Be(avatar);
        }


        [Test]
        public void GivenAnNonExistingPictureClaim_ShouldReturnDefaultAvatar()
        {
            //Arrange

            //Act
            var result = _claims.GetAvatar();

            //Assert
            result.Should().Be("http://www.gravatar.com/avatar/?d=identicon");
        }
    }

    public class WhenGettingTheNameFromAClaim : BaseUnitTest
    {
        private List<Claim> _claims = new List<Claim>();

        [SetUp]
        public void Setup()
        {
            _claims = Enumerable.Range(0, 3)
                .Select(i => new Claim(DataGenerator.Create<string>(), DataGenerator.Create<string>()))
                .ToList();

        }

        [TestCase("metadata",   "given",    "name",     "xml",  "metadata",     Description = "[1] [2] [3] [4]")]
        [TestCase("",           "given",    "name",     "xml",  "given",        Description = "[-] [2] [3] [4]")]
        [TestCase(null,         "given",    "name",     "xml",  "given",        Description = "[ ] [2] [3] [4]")]
        [TestCase("",           "",         "name",     "xml",  "name",         Description = "[-] [-] [3] [4]")]
        [TestCase(null,         null,       "name",     "xml",  "name",         Description = "[ ] [ ] [3] [4]")]
        [TestCase("",           "",         "",         "xml",  "xml",          Description = "[-] [-] [-] [4]")]
        [TestCase(null,         null,       null,       "xml",  "xml",          Description = "[ ] [ ] [ ] [4]")]
        [TestCase("",           "",         "",         "",     "",             Description = "[-] [-] [-] [-] values")]
        [TestCase(null,         null,       null,       null,   "",             Description = "[] [] [] [] values")]
        public void GivenNameClaims_ShouldReturnSomeName(string metadataFirstName, string givenNameClaim, string nameClaim, string xmlNameClaim, string expectedResult)
        {
            //Arrange            
            if (!string.IsNullOrWhiteSpace(metadataFirstName)) 
                _claims.Add(new Claim("https://incepted.co.uk/user_metadata", 
                                      JsonSerializer.Serialize(new UserDTO(metadataFirstName, "last name", ""))));
            if (!string.IsNullOrWhiteSpace(givenNameClaim)) _claims.Add(new Claim("given_name", givenNameClaim));
            if (!string.IsNullOrWhiteSpace(nameClaim)) _claims.Add(new Claim("name", nameClaim));
            if (!string.IsNullOrWhiteSpace(xmlNameClaim)) _claims.Add(new Claim(ClaimTypes.Name, xmlNameClaim));

            _claims.Add(new Claim(ClaimTypes.Email, "some email"));

            //Act
            var result = _claims.GetName();

            //Assert
            result.Should().Be(expectedResult);
        }


    }
}