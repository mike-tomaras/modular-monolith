using Bunit;
using FluentAssertions;
using Incepted.Client.Shared.Dialogs;
using Incepted.Domain.Companies.Entities;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using MudBlazor;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incepted.Client.Tests.Unit.Pages.Dialogs;

public class EditAssigneesDialogTestBase : BaseRazorUnitTest
{
    protected IEnumerable<EmployeeDTO> expectedEmployees;
    protected List<EmployeeDTO> initialAssignees;
    protected MockHttpMessageHandler http;
    protected IRenderedComponent<MudDialogProvider> CUT;
    private Company company;
    protected DealSubmissionDTO deal;

    [SetUp]
    public void Setup()
    {
        company = DataGenerator.Company();
        expectedEmployees = company.Employees.Select(Employee.Factory.ToDTO).ToList();
        initialAssignees = expectedEmployees.Take(2).ToList();
        var assignees = company.Employees
            .Take(2) //use the first two employees as deal assignees
            .Select(e => new Assignee(e.Id, e.UserId, e.Name, e.Email)).ToList();
        deal = DealSubmission.Factory.ToDTO(DataGenerator.DealSubmissions(company.Id, assignees).First());
    }

    protected void SetApiData()
    {
        http = TestContext.Services.AddMockHttpClient();
        http.Expect($"/api/v1/company/employees").RespondJson(expectedEmployees);
    }

    protected async Task RenderAsync()
    {
        CUT = TestContext.RenderComponent<MudDialogProvider>();
        CUT.Markup.Trim().Should().BeEmpty();
        var service = TestContext.Services.GetService<IDialogService>() as DialogService;
        service.Should().NotBe(null);
        IDialogReference? dialogReference = null;
        var parameters = new DialogParameters { ["DealId"] = deal.Id, ["Assignees"] = deal.Assignees.Clone() };
        await CUT.InvokeAsync(() => dialogReference = service?.Show<EditAssigneesDialog>("edit assignees", parameters));
        dialogReference.Should().NotBe(null);

        CUT.WaitForState(() => CUT.Find("h6").TextContent.Contains("Assign people to the deal"));
    }

    protected IReadOnlyList<IRenderedComponent<MudChip>> GetCurrentAssigneesChips()
    {
        return CUT.FindComponents<MudChip>();
    }

    protected Task<IEnumerable<EmployeeDTO>> SearchAutocomplete(string searchTerm)
    {
        return CUT.FindComponent<MudAutocomplete<EmployeeDTO>>().Instance.SearchFunc(searchTerm);
    }

    protected Task SelectInAutocomplete(EmployeeDTO assignee)
    {
        var autocompletecomp = CUT.FindComponent<MudAutocomplete<EmployeeDTO>>();
        var autocomplete = autocompletecomp.Instance;
        return CUT.InvokeAsync(() => autocomplete.SelectOption(assignee));
    }

    protected void RemoveAssignee(EmployeeDTO assignee)
    {
        CUT.FindComponents<MudChip>()
            .Single(c => c.Instance.Text == assignee.FullName)
            .Find("button")
            .Click();
    }
}