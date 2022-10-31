using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Incepted.Shared.DTOs;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incepted.Client.Tests.Unit.Pages.Submissions;

public class WhenLoadingTheSubmissionDrafts : SubmissionDraftsListTestBase
{
    private IEnumerable<DealListItemDTO> expectedDeals;

    [SetUp]
    public void Setup()
    {
        //Arrange
        AuthContext.SetRoles("Broker");
        expectedDeals = DealSeedData;
        SetApiData();

        // Act
        Render();
    }

    [Test]
    public void ShouldHaveOneRowPerDeal()
    {
        // Assert
        CUT.Should().NotBeNull();
        var rows = CUT.FindAll("tr");
        rows.Count.Should().Be(expectedDeals.Count() + 2);// +1 for the header row, +1 for the grouping
    }

    [Test]
    public void ShouldShowDealNames()
    {
        // Assert
        var nameCells = FindAllCellsWithLabel("Name");
        expectedDeals.All(deal =>
        {
            nameCells
                .SingleOrDefault(c => c.InnerHtml == deal.Name)
                .Should()
                .NotBeNull($"Could not find a deal-row match for deal with name: {deal.Name}.");
            return true;
        });
    }

    [Test]
    public void ShouldShowDealAssignees()
    {
        // Assert
        var assigneeCells = FindAllCellsWithLabel("AssignedTo");
        assigneeCells.Count.Should().Be(expectedDeals.Count());
        for (var i = 0; i < assigneeCells.Count; i++)
        {
            var expectedDeal = expectedDeals.ElementAt(i);
            var avatars = GetAvatarsFromCell(assigneeCells[i]);
            expectedDeal.Assignees.All(asgn =>
            {
                avatars.SingleOrDefault(a => a.InnerHtml == asgn.Initials)
                .Should().NotBeNull($"No avatar with initials '{asgn.Initials}' found.");
                return true;
            });
        }
    }

    [Test]
    public void ShouldShowDealEV()
    {
        // Assert
        var evCells = FindAllCellsWithLabel("EnterpriseValue");
        for (int i = 0; i < expectedDeals.Count(); i++)
        {
            var deal = expectedDeals.ToList()[i];
            var evCell = evCells[i];
            evCell.InnerHtml.Should().Contain(deal.EnterpriseValue.ToString(), $"at index {i}");
        }
    }
}

public class WhenSelectingToViewASubmissionDraft : SubmissionDraftsListTestBase
{
    private List<DealListItemDTO> deals;

    [SetUp]
    public void Setup()
    {
        //Arrange
        AuthContext.SetRoles("Broker");
        deals = DealSeedData;
        SetApiData();

        // Act
        TestContext.Services.GetRequiredService<FakeNavigationManager>().NavigateTo("your-submissions");//to make the component's _page variable get the right value
        Render();
    }

    [Test]
    public async Task ShouldRedirectToTheDealDetailsAsync()
    {
        //Arrange
        var expectedDealId = deals.First().Id;

        //Act
        CUT.FindAll("td[data-label='Name']").First().Click();

        // Assert
        await ShouldNavigateToAsync($"http://localhost/your-submissions/{expectedDealId}");
    }
}

public class WhenAddingANewSubmissionDraft : SubmissionDraftsListTestBase
{
    [Test]
    public void ShouldShowAddDialog()
    {
        //Arrange
        AuthContext.SetRoles("Broker");
        SetApiData();
        var dialog = SetupDialogs();
        Render();

        // Act
        CUT.Find("#addDeal").Click();
        dialog.WaitForElement("#newDealName", TimeSpan.FromSeconds(3));//wait for add deal dialog

        // Assert
        dialog.Markup.Trim().Should().NotBeEmpty();
        dialog.Find("#newDealName").Should().NotBeNull();
    }
}
