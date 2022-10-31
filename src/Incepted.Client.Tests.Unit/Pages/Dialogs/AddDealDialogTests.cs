using AutoFixture;
using Bunit;
using FluentAssertions;
using Incepted.Shared;
using Incepted.Shared.Tests.Unit.DataSeeding;
using MudBlazor;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Incepted.Client.Tests.Unit.Pages.Dialogs;

public class WhenLoadingTheAddDealDialog : AddDealDialogTestBase
{
    private IRenderedComponent<MudDialogProvider> CUT;

    [SetUp]
    public async Task SetupAsync()
    {
        //Arrange
        //Act
        CUT = await RenderAsync();
    }

    [Test, Order(1)]
    public void ShouldShowTheDialogCorrectly()
    {
        //Assert
        CUT.Find("#newDealName").Should().NotBeNull();
        var dialogButton = CUT.Find("#dialogSubmit");
        dialogButton.Should().NotBeNull();
        dialogButton.HasAttribute("disabled").Should().BeTrue();        
    }

    [Test, Order(2)]
    public void ShouldCloseOnCancel()
    {
        //Assert
        CUT.Find("#dialogCancel").Click();
        CUT.Markup.Trim().Should().BeEmpty();
    }
}

public class WhenAddingANewDealDialog : AddDealDialogTestBase
{
    [Test, Ignore("TODO: fix")]
    public async Task GivenNoErrors_ShouldAddAndNavigateToTheNewDealAsync()
    {
        //Arrange
        var newDealId = Guid.NewGuid();
        var newDealName = DataGenerator.Fixture.Create<string>();
        var http = TestContext.Services.AddMockHttpClient();
        http.Expect(HttpMethod.Post, "/api/v1/deals")
            .WithFormData(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("name", newDealName) })
            .RespondString(newDealId.ToString(), HttpStatusCode.Created);

        //Act
        var CUT = await RenderAsync();
        EnterValueInInput(CUT.Find("#newDealName"), newDealName);
        SubmitDialog(CUT);

        //Assert
        DialogShouldDissapear(CUT);
        http.VerifyNoOutstandingExpectation();
        await ShouldNavigateToAsync($"/submission-drafts/{newDealId}");
    }

    [Test]
    public async Task GivenAnError_ShouldShowTheErrorAsync()
    {
        //Arrange
        var errorMessage = DataGenerator.Fixture.Create<string>();
        var error = DealErrorCodes.DealNotFound with { errors = new ErrorCodeDetail(new List<string> { errorMessage }) };
        var http = TestContext.Services.AddMockHttpClient();
        http.Expect(HttpMethod.Post, "/api/v1/deals")
            .RespondJson(error, HttpStatusCode.BadRequest);
        SetupSnackBar();

        //Act
        var initialSnackBarMessageCount = CurrentSnackbarMessageCount();
        var CUT = await RenderAsync();
        EnterValueInInput(CUT.Find("#newDealName"), "some deal name that will fail");
        SubmitDialog(CUT);

        //Assert
        SnackbarShoulHaveAnExtraMessage(initialSnackBarMessageCount);
        SnackbarShouldContainMessage(errorMessage);
    }
}
