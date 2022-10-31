using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Incepted.API.Tests.Integration;

public class WhenCallingAnAuthedEndpointWithoutAnAccessToken
{
    private System.Net.Http.HttpClient _client;

    [OneTimeSetUp]
    public void BaseOneTimeSetup()
    {
        var application = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                //no custom auth
            });
        });

        _client = application.CreateClient();
    }

    [Test]
    public async Task ShouldReturnNotAuthed()
    {
        //Arrange

        //Act
        var response = await _client.GetAsync("api/v1/deals");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public class WhenCallingAnAuthedEndpointWithoutTheCorrectRole
{
    private System.Net.Http.HttpClient _client;

    [OneTimeSetUp]
    public void BaseOneTimeSetup()
    {
        var application = new WebApplicationFactory<Program>()
        .WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandlerWithNoRole>("Test", options => { });
            });
        });

        _client = application.CreateClient();
    }

    [Test]
    public async Task ShouldReturnNotAuthed()
    {
        //Arrange

        //Act
        //Call an endpoint with a role auth attribute
        var response = await _client.GetAsync("api/v1/deals");

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

internal class TestAuthHandlerWithNoRole : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandlerWithNoRole(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] {
            new Claim(ClaimTypes.Name, "Test user"),
            //new Claim(ClaimTypes.Role, "Broker"),  NO ROLE
            new Claim(ClaimTypes.NameIdentifier, "userId:12345"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
