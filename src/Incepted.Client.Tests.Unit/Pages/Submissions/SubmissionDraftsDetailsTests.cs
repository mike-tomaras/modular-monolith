using NUnit.Framework;

namespace Incepted.Client.Tests.Unit.Pages.Submissions;

public class WhenLoadingTheDetailsOfASubmissionDraft : SubmissionDraftDetailsTestBase
{
    [Test]
    public void ShouldShowTheDealDetails()
    {
        ////Arrange
        //AuthContext.SetRoles("Broker");
        //SetApiData();

        //// Act
        //Render();

        // Assert
        //CUT.Find("#dealName").InnerHtml.Should().Be(expectedDeal.Name);
        //CUT.Find("#dealEV").InnerHtml.Should().Be(expectedDeal.EnterpriseValue.ToString());
        //ShouldShowDealFIles();
    }

    //private void ShouldShowDealFIles()
    //{
    //    var fileRows = CUT.FindAll("#dealFiles tbody tr");
    //    fileRows.Should().HaveCount(expectedDeal.Files.Count());
    //    for (int i = 0; i < fileRows.Count(); i++)
    //    {
    //        var row = fileRows[i];
    //        var file = expectedDeal.Files.ToList()[i];
    //        row.InnerHtml.Should().Contain(file.FileName);
    //        row.InnerHtml.Should().Contain(file.LastModified.ToLocalTime().ToString("g"));
    //    }
    //}
}

//public class WhenLoadingTheDetailsOfAPresubmittedDealWithNoFiles : PresubmittedDealsDetailsTestBase
//{
//    [Test]
//    public void ShouldShowTheNoDealFilesUI()
//    {
//        //Arrange
//        AuthContext.SetRoles("Broker");
//        var dealWithNoFiles = DealSeedData.First() with { Files = ImmutableList.Create<FileDTO>() };
//        SetApiData(dealWithNoFiles);

//        // Act
//        Render();

//        // Assert
//        var noFileUI = CUT.Find("#dealFiles");
//        noFileUI.InnerHtml.Should().Contain("There are no files uploaded yet!");
//        noFileUI.InnerHtml.Should().Contain("Upload some to get started.");
//        CUT.Find("label[for='fileInput']").Should().NotBeNull();
//    }
//}

//public class WhenEditingTheEvOfADeal : PresubmittedDealsDetailsTestBase
//{
//    [Test]
//    public void ShouldShowAddDialog()
//    {
//        //Arrange
//        AuthContext.SetRoles("Broker");
//        SetApiData();
//        var dialog = SetupDialogs();
//        Render();

//        // Act
//        CUT.Find("#editEv").Click();
//        dialog.WaitForElement("#editEvAmount", TimeSpan.FromSeconds(3));//wait for dialog

//        // Assert
//        dialog.Markup.Trim().Should().NotBeEmpty();
//        dialog.Find("#editEvAmount").Should().NotBeNull();        
//    }
//}

public class WhenAssigningColleaguesToTheDealFromTheDetailsPage : SubmissionDraftDetailsTestBase
{
    [Test]
    public void ShouldShowAddDialog()
    {
        ////Arrange
        //AuthContext.SetRoles("Broker");
        //SetApiData();
        //var dialog = SetupDialogs();
        //Render();

        //// Act
        //CUT.Find("#assign").Click();

        //// Assert
        //dialog.Markup.Trim().Should().NotBeEmpty();
        //dialog.Find("#assigneesSummary").Should().NotBeNull();
    }
}