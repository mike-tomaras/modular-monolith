using Bunit;
using FluentAssertions;
using Incepted.Client.Shared;
using NUnit.Framework;
using System.Security.Claims;

namespace Incepted.Client.Tests.Unit.Shared;

public class MainLayoutTests : BaseRazorUnitTest
{
    [SetUp]
    public void Setup()
    {
        AuthContext.SetClaims(new Claim("https://incepted.co.uk/roles", "role"));
    }

    [Test]
    public void DrawerCanToggle()
    {        
        // Act
        var CUT = TestContext.RenderComponent<MainLayout>();

        // Assert
        CUT.Find("#drawerImage").GetAttribute("src").Should().Contain("logo-icon");
        
        // Act
        CUT.Find("#drawerToggleButton").Click();

        // Assert
        CUT.Find("#drawerImage").GetAttribute("src").Should().Contain("logo-text");
    }
}
