using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.Enums;
using Syncfusion.XlsIO;
using IXlStyle = Syncfusion.XlsIO.IStyle;

namespace Incepted.DocGen.ExcelHelpers;

internal static class ExcelWarrantiesHelpers
{
    private static int ColumnShift(int index) => index * 4;
    private static int SeparatorCol(int index) => 2 + ColumnShift(index);
    private static int CoverageCol(int index) => 3 + ColumnShift(index);
    private static int KnowledgeScrapeCol(int index) => 4 + ColumnShift(index);
    private static int CommentCol(int index) => 5 + ColumnShift(index);
    private static ExcelKnownColors CoverageColor(CoveragePosition coverage) =>
        coverage switch 
        { 
            CoveragePosition.Yes => ExcelKnownColors.Light_green,
            CoveragePosition.Partial => ExcelKnownColors.Light_yellow,
            CoveragePosition.TBC => ExcelKnownColors.Grey_25_percent,
            CoveragePosition.No => ExcelKnownColors.White,
            CoveragePosition.None => ExcelKnownColors.White,
            _ => throw new ArgumentException($"You have not specified a color for the coverage '{coverage}' when downloading the excel")
        };
    private static ExcelKnownColors KnowledgeScrapeColor(KnowledgeScrape knowledgeScrape) =>
        knowledgeScrape switch
        {
            KnowledgeScrape.Yes => ExcelKnownColors.Light_green,
            KnowledgeScrape.Partial => ExcelKnownColors.Light_yellow,
            KnowledgeScrape.No => ExcelKnownColors.White,
            KnowledgeScrape.None => ExcelKnownColors.White,
            _ => throw new ArgumentException($"You have not specified a color for the knowledge scrape '{knowledgeScrape}' when downloading the excel")
        };

    public static void AddWarrantiesTableValues(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle normalCellStyle, int index = 0)
    {
        var warranties = feedback.Warranties.OrderBy(w => w.Order).ToImmutable();
        var startRow = 4;
        var lastRow = warranties.Any() ? startRow + warranties.Count() - 1 : 4;

        for (int i = 0; i < warranties.Count(); i++)
        {
            var warranty = warranties[i];

            sheet.Range[startRow + i, CoverageCol(index)].Text = warranty.CoveragePosition.ToString();
            sheet.Range[startRow + i, KnowledgeScrapeCol(index)].Text = warranty.KnowledgeScrape.ToString();
            sheet.Range[startRow + i, CommentCol(index)].Text = warranty.Comment;
        }

        sheet.Range[startRow, SeparatorCol(index), lastRow, CommentCol(index)].CellStyle = normalCellStyle;
        for (int i = 0; i < warranties.Count(); i++)
        {
            var warranty = warranties[i];
            sheet.Range[startRow + i, CoverageCol(index)].CellStyle.ColorIndex = CoverageColor(warranty.CoveragePosition);
            sheet.Range[startRow + i, KnowledgeScrapeCol(index)].CellStyle.ColorIndex = KnowledgeScrapeColor(warranty.KnowledgeScrape);
        }
        sheet.Range[startRow - 1, SeparatorCol(index), lastRow, KnowledgeScrapeCol(index)].AutofitColumns();
        sheet.Range[startRow, CommentCol(index), lastRow, CommentCol(index)].ColumnWidth = 30;
        sheet.Range[startRow, CommentCol(index), lastRow, CommentCol(index)].WrapText = true;
        sheet.Range[startRow, SeparatorCol(index), lastRow, SeparatorCol(index)].CellStyle.ColorIndex = ExcelKnownColors.Black;
    }

    public static void AddWarrantiesTableValueHeaders(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle headerStyle, int index = 0)
    {   
        sheet.Range[2, CoverageCol(index)].Text = feedback.InsuranceCompanyName;
        sheet.Range[2, CoverageCol(index)].CellStyle.Font.Bold = true;
        sheet.Range[2, CoverageCol(index)].CellStyle.Font.Size = 13;
        sheet.Range[3, CoverageCol(index)].Text = "Coverage Position";
        sheet.Range[3, KnowledgeScrapeCol(index)].Text = "Knowledge Scrape";
        sheet.Range[3, CommentCol(index)].Text = "Comment";

        sheet.Range[3, SeparatorCol(index), 3, CommentCol(index)].CellStyle = headerStyle;
        sheet.Range[3, SeparatorCol(index)].ColumnWidth = 0.3;
        sheet.Range[3, SeparatorCol(index)].CellStyle.ColorIndex = ExcelKnownColors.Black;
    }

    public static void AddWarrantiesTable(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle headerStyle, IXlStyle normalCellStyle)
    {
        sheet.Range["A3"].Text = "Clause";
        sheet.Range["A3"].CellStyle = headerStyle;

        var warranties = feedback.Warranties.OrderBy(w => w.Order).ToImmutable();
        for (int i = 0; i < warranties.Count(); i++)
        {
            var warranty = warranties[i];

            sheet.Range[$"A{4 + i}"].Text = warranty.Description;
        }

        var lastRow = warranties.Any() ? 3 + warranties.Count : 4;
        sheet.Range[$"A4:D{lastRow}"].CellStyle = normalCellStyle;
        sheet.Range[$"A3:C{lastRow}"].ColumnWidth = 30;
        sheet.Range[$"A3:C{lastRow}"].WrapText = true;
    }
}