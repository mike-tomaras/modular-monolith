using Bunit;
using FluentAssertions;
using Incepted.Client.Pages.Submissions.Dialogs;
using MudBlazor;
using System.Threading.Tasks;

namespace Incepted.Client.Tests.Unit.Pages.Dialogs;

public class AddDealDialogTestBase : BaseRazorUnitTest
{
    protected async Task<IRenderedComponent<MudDialogProvider>> RenderAsync()
    {
        var CUT = TestContext.RenderComponent<MudDialogProvider>();
        CUT.Markup.Trim().Should().BeEmpty();
        var service = TestContext.Services.GetService<IDialogService>() as DialogService;
        service.Should().NotBe(null);
        IDialogReference? dialogReference = null;
        
        await CUT.InvokeAsync(() => dialogReference = service?.Show<AddDealDialog>("add deal"));
        dialogReference.Should().NotBe(null);

        CUT.WaitForState(() => CUT.Find("h6").TextContent.Contains("Add a new project"));

        return CUT;
    }
}