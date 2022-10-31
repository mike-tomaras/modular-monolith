using Incepted.DocGen.DocHelpers;
using Incepted.DocGen.ExcelHelpers;
using Incepted.Shared.DTOs;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Drawing;
using Syncfusion.XlsIO;

namespace Incepted.DocGen.SyncFusion;

public class SyncFusionDocGeneratorService : IDocGenService
{
    public Stream GetNBIDoc(SubmissionFeedbackDTO feedback, string insurerName, string tcsFileName)
    {
        WordDocument document = new();

        var section = (WSection)document.AddSection();
        section.PageSetup.Margins.All = 72;
        section.PageSetup.PageSize = new SizeF(612, 792);
        DocNbiHelpers.SetStyles(document);
        DocNbiHelpers.AddFooter(section);
        DocNbiHelpers.AddTopLevelHeader(feedback, insurerName, section);


        DocNbiHelpers.AddParagraph("Note: This document is a summary of your terms " +
            "that have already been provided to the broker.", section, isItalic: true);
        
        
        DocNbiHelpers.AddHeader("1. Pricing Structure", section);
        DocNbiHelpers.AddParagraph($"Please refer to annex: \"Annex A - Pricing and Warranties.xlsx\", sheet \"Pricing Structure\".", section);


        DocNbiHelpers.AddHeader("2. Enhancements", section);
        var enhancements = feedback.Enhancements.Where(e => e.Type == Shared.Enums.EnhancementType.Request && e.InsurerOffersIt);
        if (enhancements.Any())
        {
            DocNbiHelpers.AddParagraph("We offer the following enhancements", section);
            DocNbiHelpers.AddEnhancementsList(section, enhancements);
        }
        else
        {
            DocNbiHelpers.AddParagraph("None offered.", section);
        }        


        DocNbiHelpers.AddHeader("3. Assumptions", section);
        enhancements = feedback.Enhancements.Where(e => e.Type == Shared.Enums.EnhancementType.Assumption && e.InsurerOffersIt);
        if (enhancements.Any())
        {
            DocNbiHelpers.AddParagraph("We agree with the following assumptions", section);
            DocNbiHelpers.AddEnhancementsList(section, enhancements);
        }
        else
        {
            DocNbiHelpers.AddParagraph("No assumptions.", section);
        }


        DocNbiHelpers.AddHeader("4. Warranties", section);
        DocNbiHelpers.AddParagraph($"Please refer to annex: \"Annex A - Pricing and Warranties.xlsx\", sheet \"Warranties\".", section);
        
        
        DocNbiHelpers.AddHeader("5. Exclusions", section);
        var exclusions = feedback.Exclusions.Where(e => e.InsurerRequiresIt);
        if (exclusions.Any())
        {
            DocNbiHelpers.AddParagraph("We require the following exclusions", section);
            DocNbiHelpers.AddExclusionsList(section, exclusions);
        }
        else
        {
            DocNbiHelpers.AddParagraph("No exclusions.", section);
        }
        DocNbiHelpers.AddHeader("Excluded countries", section, level: 3);
        DocNbiHelpers.AddParagraph($"These countries are excluded from our UW: " +
            $"{feedback.ExcludedCountries.Aggregate(string.Empty, (current, next) => $"{current}, {next}").TrimStart(',')}", section);


        DocNbiHelpers.AddHeader("6. UW focus", section);
        DocNbiHelpers.AddParagraph($"This is our UW focus: {feedback.UwFocus.Aggregate(string.Empty, (current, next) => $"{current}, {next}").TrimStart(',')}", section);
        
        
        DocNbiHelpers.AddHeader("7. Additional notes", section);
        if (string.IsNullOrEmpty(feedback.Notes))
        {
            DocNbiHelpers.AddParagraph("No additional notes", section);
        }
        else
        {
            DocNbiHelpers.AddParagraph(feedback.Notes, section);
        }

        if (!string.IsNullOrEmpty(tcsFileName))
        {
            DocNbiHelpers.AddHeader("8. Terms & Conditions", section);
            DocNbiHelpers.AddParagraph($"Please refer to annex: \"Annex B - T&Cs - {tcsFileName}\"", section);
        }        


        section.AddParagraph();


        //Saves the Word document to  MemoryStream
        MemoryStream stream = new MemoryStream();
        document.Save(stream, FormatType.Docx);
        stream.Position = 0;

        return stream;
    }

    public Stream GetNBISheet(SubmissionFeedbackDTO feedback)
    {
        using var excelEngine = new ExcelEngine();
        IApplication application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        IWorkbook workbook = application.Workbooks.Create(0);
        var headerStyle = ExcelStyilingHelpers.CreateHeaderCellStyle(workbook);
        var normalCellStyle = ExcelStyilingHelpers.CreateNormalCellStyle(workbook);

        var ev = feedback.Pricing.EnterpriseValue;



        IWorksheet pricingSheet = workbook.Worksheets.Create("Pricing Structure");

        ExcelPricingTableHelpers.AddPricingTableOptionHeaders(feedback, ev, pricingSheet, headerStyle);
        ExcelPricingTableHelpers.AddPricingTableOptions(feedback, ev, pricingSheet, normalCellStyle);

        ExcelPricingTableHelpers.AddPricingTableValuesHeaders(feedback, ev, pricingSheet, headerStyle);
        ExcelPricingTableHelpers.AddPricingTableValues(feedback, ev, pricingSheet, normalCellStyle);

        ExcelPricingTableHelpers.AddPricingEnhancementsTable(feedback, pricingSheet, headerStyle, normalCellStyle);



        IWorksheet warrantySheet = workbook.Worksheets.Create("Warranties");

        ExcelWarrantiesHelpers.AddWarrantiesTable(feedback, warrantySheet, headerStyle, normalCellStyle);
        ExcelWarrantiesHelpers.AddWarrantiesTableValueHeaders(feedback, warrantySheet, headerStyle);
        ExcelWarrantiesHelpers.AddWarrantiesTableValues(feedback, warrantySheet, normalCellStyle);

        //Save the workbook as stream
        MemoryStream stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }

    public Stream GetNBIComparisonSheet(List<SubmissionFeedbackDTO> feedbacks)
    {
        //this code assumes that all feedbacks have the same options (limit/retention combos) 
        //as this is a business rule at the time of this code's writing

        using var excelEngine = new ExcelEngine();
        IApplication application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        IWorkbook workbook = application.Workbooks.Create(0);
        var headerStyle = ExcelStyilingHelpers.CreateHeaderCellStyle(workbook);
        var normalCellStyle = ExcelStyilingHelpers.CreateNormalCellStyle(workbook);

        var ev = feedbacks.First().Pricing.EnterpriseValue;



        IWorksheet pricingSheet = workbook.Worksheets.Create("Pricing Structure");

        ExcelPricingTableHelpers.AddPricingTableOptionHeaders(feedbacks.First(), ev, pricingSheet, headerStyle);
        ExcelPricingTableHelpers.AddPricingTableOptions(feedbacks.First(), ev, pricingSheet, normalCellStyle);
        for (int i = 0; i < feedbacks.Count(); i++)
        {
            var feedback = feedbacks[i];
            ExcelPricingTableHelpers.AddPricingTableValuesHeaders(feedback, ev, pricingSheet, headerStyle, index: i);
            ExcelPricingTableHelpers.AddPricingTableValues(feedback, ev, pricingSheet, normalCellStyle, index: i);
        }



        IWorksheet enhancementsSheet = workbook.Worksheets.Create("Enhancements");

        ExcelEnhancementsHelpers.AddEnhancementsTable(feedbacks.First(), enhancementsSheet, headerStyle, normalCellStyle);
        for (int i = 0; i < feedbacks.Count(); i++)
        {
            var feedback = feedbacks[i];
            ExcelEnhancementsHelpers.AddEnhancementsTableValuesHeaders(feedback, enhancementsSheet, headerStyle, index: i);
            ExcelEnhancementsHelpers.AddEnhancementsTableValues(feedback, enhancementsSheet, normalCellStyle, index: i);
        }



        IWorksheet warrantySheet = workbook.Worksheets.Create("Warranties");

        ExcelWarrantiesHelpers.AddWarrantiesTable(feedbacks.First(), warrantySheet, headerStyle, normalCellStyle);
        for (int i = 0; i < feedbacks.Count(); i++)
        {
            var feedback = feedbacks[i];
            ExcelWarrantiesHelpers.AddWarrantiesTableValueHeaders(feedback, warrantySheet, headerStyle, index: i);
            ExcelWarrantiesHelpers.AddWarrantiesTableValues(feedback, warrantySheet, normalCellStyle, index: i);
        }



        IWorksheet exclusionSheet = workbook.Worksheets.Create("Exclusions");

        ExcelExclusionsHelpers.AddExclusionsTable(feedbacks, exclusionSheet, headerStyle, normalCellStyle);
        for (int i = 0; i < feedbacks.Count(); i++)
        {
            var feedback = feedbacks[i];
            ExcelExclusionsHelpers.AddExclusionsTableValuesHeaders(feedback, exclusionSheet, headerStyle, index: i);
            ExcelExclusionsHelpers.AddExclusionsTableValues(feedback, exclusionSheet, normalCellStyle, index: i);
        }

        IWorksheet uwFocusSheet = workbook.Worksheets.Create("UW focus");

        ExcelUwFocusHelpers.AddUwFocusTable(feedbacks, uwFocusSheet, headerStyle, normalCellStyle);
        for (int i = 0; i < feedbacks.Count(); i++)
        {
            var feedback = feedbacks[i];
            ExcelUwFocusHelpers.AddUwFocusTableValuesHeaders(feedback, uwFocusSheet, headerStyle, index: i);
            ExcelUwFocusHelpers.AddUwFocusTableValues(feedback, uwFocusSheet, normalCellStyle, index: i);
        }


        IWorksheet notesSheet = workbook.Worksheets.Create("Notes from insurers");

        for (int i = 0; i < feedbacks.Count(); i++)
        {
            var feedback = feedbacks[i];
            ExcelNotesHelpers.AddNotesTableValuesHeaders(feedback, notesSheet, headerStyle, index: i);
            ExcelNotesHelpers.AddNotesTableValues(feedback, notesSheet, normalCellStyle, index: i);
        }

        //Save the workbook as stream
        MemoryStream stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return stream;
    }
}