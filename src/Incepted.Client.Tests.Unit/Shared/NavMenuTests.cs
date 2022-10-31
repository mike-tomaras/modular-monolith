using Bunit;
using FluentAssertions;
using Incepted.Client.Shared;
using NUnit.Framework;

namespace Incepted.Client.Tests.Unit.Shared;

public class NavMenuTests : BaseRazorUnitTest
{
    [SetUp]
    public void Setup()
    {
        
    }

    [Test]
    public void ShouldShowAGreetingToAUSer()
    {        
        // Act
        var CUT = TestContext.RenderComponent<NavMenu>(parameters => parameters
                .Add(p => p.DrawerOpen, true)
            );
        
        // Assert
        CUT.Find("#userGreeting").InnerHtml.Should().Be("Hello, TEST USER!");
    }

}
