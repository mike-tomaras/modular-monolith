using AngleSharp.Dom;
using Bunit;
using Incepted.Client.Pages.Submissions;
using Incepted.Domain.Deals.Domain;
using Incepted.Shared.DTOs;
using Incepted.Shared.Tests.Unit.DataSeeding;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incepted.Client.Tests.Unit.Pages.Submissions;

public class SubmissionDraftDetailsTestBase : BaseRazorUnitTest
{
    protected DealSubmissionDTO expectedDeal;
    protected MockHttpMessageHandler http;
    protected IRenderedComponent<SubmissionDraftDetails> CUT;
    protected List<DealSubmissionDTO> DealSeedData;

    [SetUp]
    public void Setup()
    {
        DealSeedData = DataGenerator.DealSubmissions().Select(DealSubmission.Factory.ToDTO).ToList();
    }

    public void SetApiData(DealSubmissionDTO? deal = null)
    {
        if (deal == null) deal = DealSeedData.First();

        expectedDeal = deal;
        http = TestContext.Services.AddMockHttpClient();
        http.Expect($"/api/v1/deals/{expectedDeal.Id}").RespondJson(expectedDeal);
    }

    public async Task RenderAsync()
    {
        CUT = TestContext.RenderComponent<SubmissionDraftDetails>(parameters =>
            parameters.Add(p => p.DealId, expectedDeal.Id));
        await Task.Delay(500);
        CUT.Render();
        CUT.WaitForState(() => CUT.Find("h6").TextContent == "Project Name");
    }
}