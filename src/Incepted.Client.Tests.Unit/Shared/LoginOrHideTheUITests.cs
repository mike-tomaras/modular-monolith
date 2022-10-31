using Bunit.TestDoubles;
using FluentAssertions;
using Incepted.Client.Shared;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using NUnit.Framework;

namespace Incepted.Client.Tests.Unit.Shared;

public class LoginOrHideTheUITests
{
    [Test]
    public void WhenIsLoggedIn_ShouldNotRedirectToLogin()
    {
        //Arrange
        var testContext = new Bunit.TestContext();
        var authContext = testContext.AddTestAuthorization();
        authContext.SetAuthorized("TEST USER");
        testContext.Services.AddMudServices();

        // Act
        testContext.RenderComponent<LoginOrHideTheUI>();

        // Assert
        var navMan = testContext.Services.GetRequiredService<FakeNavigationManager>();
        navMan.Uri.Should().Be("http://localhost/");
    }

    [Test]
    public void WhenNotLoggedIn_ShouldRedirectToLogin()
    {
        //Arrange
        var testContext = new Bunit.TestContext();
        var authContext = testContext.AddTestAuthorization();
        testContext.Services.AddMudServices();

        // Act
        testContext.RenderComponent<LoginOrHideTheUI>();

        // Assert
        var navMan = testContext.Services.GetRequiredService<FakeNavigationManager>();
        navMan.Uri.Should().Be("http://localhost/authentication/login");
    }

}
