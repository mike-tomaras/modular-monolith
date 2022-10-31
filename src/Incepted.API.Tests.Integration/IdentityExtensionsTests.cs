using FluentAssertions;
using Incepted.Shared.ValueTypes;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Optional;
using Serilog;
using Serilog.Sinks.TestCorrelator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Incepted.API.Tests.Integration;

public class WhenGettingTheUserIdFromTheHttpContext
{
    private Mock<IHttpContextAccessor> mockHttpContextAccessor;
    private UserId expectedUserId = new UserId("auth0|12345");

    [Test]
    public void GivenThereIsAUserIdClaim_ShouldReturnSomeUserId()
    {
        //Arrange
        mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.Value),
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        context.User = new ClaimsPrincipal(identity);
        mockHttpContextAccessor.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns(context);

        //Act
        var response = mockHttpContextAccessor.Object.GetAuthIdFromAccessToken();

        //Assert
        response
            .ValueOr(() => throw new Exception($"This test will fail here because there should be a result and not {response}"))
            .Value
            .Should().Be(expectedUserId.Value);
    }
    
    [Test]
    public void GivenThereIsNoUserIdClaim_ShouldReturnNoneAndLogAWarning()
    {
        //Arrange
        mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();
        var claims = new List<Claim>{
            new Claim(ClaimTypes.Name, "abc"),
            new Claim(ClaimTypes.Email, "def@def.com"),
        }; // no user id claim (NameIdentifier)
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        context.User = new ClaimsPrincipal(identity);
        mockHttpContextAccessor.Setup(httpContextAccessor => httpContextAccessor.HttpContext).Returns(context);
        var expectedLogClaims =
            $"{claims.First().Type}={claims.First().Value}, " +
            $"{claims.Last().Type}={claims.Last().Value}";
        Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

        //Act
        using var logTest = TestCorrelator.CreateContext();
        var response = mockHttpContextAccessor.Object.GetAuthIdFromAccessToken();

        //Assert
        response.Should().Be(Option.None<UserId>());
        
        var logEvent = TestCorrelator.GetLogEventsFromCurrentContext();
        logEvent.Should().ContainSingle();
        logEvent.First().MessageTemplate.Text
           .Should().Be("Attempt to access the API with no valid Auth Id Claim. {ClaimNames}");
        logEvent.First().Properties.Should().HaveCount(1);
        logEvent.First().Properties.First().Key.Should().Be("ClaimNames");
        logEvent.First().Properties.First().Value.ToString().Trim('"').Should().Be(expectedLogClaims);
    }
}