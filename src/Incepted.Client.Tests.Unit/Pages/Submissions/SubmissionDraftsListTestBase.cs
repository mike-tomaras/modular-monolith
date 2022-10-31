using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Incepted.Client.Pages.Submissions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Linq;

namespace Incepted.Client.Tests.Unit.Pages.Submissions;
public class SubmissionDraftsListTestBase : BaseRazorUnitTest
{
    protected MockHttpMessageHandler http;
    protected IRenderedComponent<SubmissionDraftsList> CUT;
    protected List<DealListItemDTO> DealSeedData;

    [SetUp]
    public void CommonSetup()
    {
        DealSeedData = DataGenerator.DealSubmissions().Select(DealSubmission.Factory.ToListItemDTO).ToList();
    }

    public void SetApiData()
    {
        http = TestContext.Services.AddMockHttpClient();
        http.Expect("/api/v1/deals").RespondJson(DealSeedData);
    }

    public void Render()
    {
        CUT = TestContext.RenderComponent<SubmissionDraftsList>();
        CUT.WaitForElement("[data-label=\"Name\"]");//wait for built in progress animation to finish
    }

    public IRefreshableElementCollection<IElement> FindAllCellsWithLabel(string columnName)
    {
        return CUT.FindAll($"[data-label=\"{columnName}\"]");
    }
    public IRefreshableElementCollection<IElement> FindAllActionCells()
    {
        return CUT.FindAll($"[data-label=\"Actions\"]");
    }

    public IEnumerable<IElement> GetAvatarsFromCell(IElement avatarCell)
    {
        return avatarCell.Children[0].Children[0].Children.Where(c => c.ClassList.Contains("mud-avatar"));
    }

    public void ActionShouldExist(IElement actionCell, int cellIndex, string actionName)
    {
        actionCell.Children.SingleOrDefault(c => c.TagName == "BUTTON" && c.GetAttribute("aria-label") == actionName)
            .Should().NotBeNull($"No '{actionName}' action found for row index {cellIndex}");
    }
}