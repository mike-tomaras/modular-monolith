using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Incepted.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Incepted.Client.Tests.Unit;

[TestFixture]
[Parallelizable(scope: ParallelScope.Fixtures)]
[Ignore("Temporarily while UI is in flux. TODO: enable and cover UI properly")]
public class BaseRazorUnitTest
{
    public Bunit.TestContext TestContext { get; set; }
    public TestAuthorizationContext AuthContext { get; set; }
    public IRenderedComponent<MudSnackbarProvider> Snackbar { get; set; }

    [SetUp]
    public void SetupBase()
    {
        TestContext = new Bunit.TestContext();
        AuthContext = TestContext.AddTestAuthorization();
        AuthContext.SetAuthorized("TEST USER");

        TestContext.Services.AddMudServices();
        TestContext.Services.AddScoped<IDealFileService, DealFileService>();
        TestContext.JSInterop.Mode = JSRuntimeMode.Loose;        
    }

    [OneTimeTearDown]
    public void TeardownBase()
    {
        TestContext.Dispose();
    }

    protected async Task ShouldNavigateToAsync(string uri)
    {
        await Task.Delay(500);
        var navMan = TestContext.Services.GetRequiredService<FakeNavigationManager>();
        (navMan.Uri == uri || navMan.History.Any(h => uri.EndsWith(h.Uri))).Should().BeTrue();
    }
    protected void SetupSnackBar()
    {
        Snackbar = TestContext.RenderComponent<MudSnackbarProvider>();
    }
    protected void SnackbarShouldContainMessage(string expectedMessage)
    {
        var snackbarMessages = Snackbar
            .FindAll("div.mud-snackbar-content-message")
            .Aggregate(string.Empty, (c, n) => c += " | " + n.InnerHtml);
        Snackbar.FindAll("div.mud-snackbar-content-message").
            Any(message => message.InnerHtml.Contains(expectedMessage))
            .Should().BeTrue($"messages found: {snackbarMessages}");
    }
    protected int? CurrentSnackbarMessageCount()
    {
        return Snackbar.FindAll("div.mud-snackbar-content-message").Count();
    }
    protected void SnackbarShoulHaveAnExtraMessage(int? initialMessageCount)
    {
        Snackbar.WaitForState(() => Snackbar?
        .FindAll("div.mud-snackbar-content-message")
        .Count == initialMessageCount + 1);
    }
    protected IRenderedComponent<MudDialogProvider> SetupDialogs()
    {
        var dialog = TestContext.RenderComponent<MudDialogProvider>();
        dialog.Markup.Trim().Should().BeEmpty();
        return dialog;
    }
    protected void SubmitDialog(IRenderedComponent<MudDialogProvider> dialog)
    {
        dialog.WaitForState(() => !dialog.Find("#dialogSubmit").HasAttribute("disabled"));
        dialog.Find("#dialogSubmit").Click();        
    }
    protected void DialogShouldDissapear(IRenderedComponent<MudDialogProvider> dialog)
    {
        dialog.WaitForState(() => string.IsNullOrEmpty(dialog.Markup.Trim()), TimeSpan.FromSeconds(5));
    }

    protected void EnterValueInInput<T>(IElement element, T value)
    {
        element.Input(value);
        element.Blur();
    }
}
