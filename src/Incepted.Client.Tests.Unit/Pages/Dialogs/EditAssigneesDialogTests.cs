using AutoFixture;
using Bunit;
using FluentAssertions;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using MudBlazor;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Incepted.Client.Tests.Unit.Pages.Dialogs;

public class WhenLoadingTheAssigneesDialog : EditAssigneesDialogTestBase
{

    [SetUp]
    public async Task SetupAsync()
    {
        //Arrange
        SetApiData();

        //Act
        await RenderAsync();
        await Task.Delay(500);//give time to get the employees, there is no element to assert that
    }

    [Test, Order(1)]
    public void ShouldShowSummaryMessage()
    {
        //Assert
        var summary = CUT.Find("#assigneesSummary");
        summary.InnerHtml.Contains("Colleagues assigned to this deal:");
        summary.InnerHtml.Contains(deal.Assignees.Count().ToString());
    }

    [Test, Order(1)]
    public void ShouldShowCurrentAssignees()
    {
        //Assert
        var selectedAssignees = GetCurrentAssigneesChips();
        selectedAssignees.Count().Should().Be(deal.Assignees.Count());
        foreach (var assignee in deal.Assignees)
            selectedAssignees.Select(chip => chip.Instance.Text).Should().Contain(assignee.FullName);
    }

    [Test, Order(1)]
    public async Task AutocompleteShouldHaveAllEmployees()
    {
        //Assert
        var result = await SearchAutocomplete(string.Empty);
        result.Count().Should().Be(expectedEmployees.Count());

        foreach (var employee in expectedEmployees)
        {
            result = await SearchAutocomplete(employee.FullName);
            result.Count().Should().Be(1);
            result.First().FullName.Should().Be(employee.FullName);
        }
    }

    [Test, Order(2)]
    public void ShouldCloseOnCancel()
    {
        //Assert
        CUT.Find("#dialogCancel").Click();
        CUT.Markup.Trim().Should().BeEmpty();
    }
}

public class WhenAddingAndRemovingAssignees : EditAssigneesDialogTestBase
{
    [SetUp]
    public async Task SetupAsync()
    {
        //Arrange
        SetApiData();

        //Act
        await RenderAsync();
    }

    [Test]
    public async Task ShouldAddNewAssigneeAsANewChipAsync()
    {
        //Arrange
        var selectedAssigneesCount = CUT.FindComponents<MudChip>().Count();
        var newAssignee = expectedEmployees.Last();

        //Act
        await SelectInAutocomplete(newAssignee);

        //Assert
        CUT.WaitForAssertion(() => CUT.FindComponents<MudChip>().Count().Should().Be(selectedAssigneesCount + 1));
        CUT.FindComponents<MudChip>().Any(c => c.Instance.Text == newAssignee.FullName).Should().BeTrue();
    }

    [Test]
    public async Task ShouldNotAddAssigneeAgainIfTheyreAlreadySelectedAsync()
    {
        //Arrange
        var selectedAssigneesCount = GetCurrentAssigneesChips().Count();
        var newAssignee = expectedEmployees.First();//existing one

        //Act
        await SelectInAutocomplete(newAssignee);

        //Assert
        CUT.WaitForAssertion(() => GetCurrentAssigneesChips().Count().Should().Be(selectedAssigneesCount));//no change        
    }

    [Test]
    public void ShouldRemoveAssigneeIfChipIsDismissed()
    {
        //Arrange
        var selectedAssigneesCount = GetCurrentAssigneesChips().Count();
        var assigneeToRemove = expectedEmployees.First();

        //Act
        RemoveAssignee(assigneeToRemove);

        //Assert
        CUT.WaitForAssertion(() => GetCurrentAssigneesChips().Count().Should().Be(selectedAssigneesCount - 1));
        GetCurrentAssigneesChips().Any(c => c.Instance.Text == assigneeToRemove.FullName).Should().BeFalse();
    }

    [Test, Ignore("TODO: fix flaky test")]
    public void ShouldDisableSaveButtonIfThereAreNoAssignees()
    {
        //Arrange


        //Act
        foreach (var assignee in initialAssignees)
        {
            RemoveAssignee(assignee);
        }

        //Assert
        CUT.WaitForState(() => CUT.Find("#dialogSubmit").HasAttribute("disabled"));
    }
}

public class WhenSavingUpdatedAssignees : EditAssigneesDialogTestBase
{
    private List<EmployeeDTO> expectedAssigneesAtTheStart;
    private List<EmployeeDTO> expectedAssigneesAtTheEnd;

    [SetUp]
    public async Task SetupAsync()
    {
        //Arrange
        expectedAssigneesAtTheStart = deal.Assignees.ToList();
        expectedAssigneesAtTheEnd = deal.Assignees.Take(1).ToList();//we will remove the last one
        var assigneeToRemove = deal.Assignees.Last();
        SetApiData();

        //Act
        SetupSnackBar();
        await RenderAsync();
        RemoveAssignee(assigneeToRemove);
    }

    [Test]
    public void GivenTheUpdateSucceeds_ShouldCloseDialog()
    {
        //Arrange
        var updateDto = new UpdateDealAssigneesDTO(deal.Id, expectedAssigneesAtTheEnd.ToImmutable());
        var updateDtoJson = JsonSerializer.Serialize(updateDto);
        http.Expect(HttpMethod.Put, "/api/v1/deals/assignees")
            .WithContent(updateDtoJson)
            .Respond(HttpStatusCode.OK);

        //Act
        CUT.Find("#dialogSubmit").Click();

        //Assert
        CUT.WaitForAssertion(() => CUT.Markup.Trim().Should().BeEmpty());
        SnackbarShouldContainMessage("Assignees updated successfully");
    }

    [Test]
    public void GivenTheUpdateFails_ShouldResetTheSelection()
    {
        //Arrange
        var errorMessage = DataGenerator.Fixture.Create<string>();
        var error = DealErrorCodes.AssigneesAreNotValid with { errors = new ErrorCodeDetail(new List<string> { errorMessage }) };
        http.Expect(HttpMethod.Put, "/api/v1/deals/assignees")
            .RespondJson(error, HttpStatusCode.BadRequest);

        //Act
        CUT.Find("#dialogSubmit").Click();

        //Assert
        CUT.WaitForAssertion(() => GetCurrentAssigneesChips().Count().Should().Be(expectedAssigneesAtTheStart.Count()), TimeSpan.FromSeconds(3));
    }
}
