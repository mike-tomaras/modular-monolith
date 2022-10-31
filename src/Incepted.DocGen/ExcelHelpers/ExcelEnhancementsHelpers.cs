using Incepted.Shared;
using Incepted.Shared.DTOs;
using Syncfusion.XlsIO;
using IXlStyle = Syncfusion.XlsIO.IStyle;

namespace Incepted.DocGen.ExcelHelpers;

internal static class ExcelEnhancementsHelpers
{
    private static int ColumnShift(int index) => index * 4;
    private static int SeparatorCol(int index) => 3 + ColumnShift(index);
    private static int OfferedCol(int index) => 4 + ColumnShift(index);
    private static int ApCol(int index) => 5 + ColumnShift(index);
    private static int CommentCol(int index) => 6 + ColumnShift(index);

    public static void AddEnhancementsTableValues(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle normalCellStyle, int index = 0)
    {        
        var enhancements = feedback.Enhancements;
        var startRow = 4;
        var lastRow = startRow + enhancements.Count() - 1;

        for (int i = 0; i < enhancements.Count(); i++)
        {
            var enhancement = enhancements[i];
            IRange enhancementPosition = sheet.FindFirst(enhancement.Title, ExcelFindType.Text);

            sheet.Range[enhancementPosition.Row, OfferedCol(index)].Text = enhancement.InsurerOffersIt ? "YES" : "NO";
            sheet.Range[startRow + i, ApCol(index)].Text = $"{enhancement.AP * 100}%";
            sheet.Range[startRow + i, CommentCol(index)].Text = enhancement.Comment;
            
        }
        
        sheet.Range[startRow, SeparatorCol(index), lastRow, CommentCol(index)].CellStyle = normalCellStyle;
        for (int i = 0; i < enhancements.Count(); i++)
        {
            var enhancement = enhancements[i];
            IRange enhancementPosition = sheet.FindFirst(enhancement.Title, ExcelFindType.Text);

            sheet.Range[enhancementPosition.Row, OfferedCol(index)].CellStyle.ColorIndex = enhancement.InsurerOffersIt ? ExcelKnownColors.Light_green : ExcelKnownColors.White;
        }
        sheet.Range[startRow - 1, SeparatorCol(index), lastRow, ApCol(index)].AutofitColumns();//include header
        sheet.Range[startRow, CommentCol(index), lastRow, CommentCol(index)].ColumnWidth = 30;
        sheet.Range[startRow, CommentCol(index), lastRow, CommentCol(index)].WrapText = true;
        sheet.Range[startRow, SeparatorCol(index), lastRow, SeparatorCol(index)].CellStyle.ColorIndex = ExcelKnownColors.Black;
    }

    public static void AddEnhancementsTableValuesHeaders(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle headerStyle, int index = 0)
    {
        sheet.Range[2, OfferedCol(index)].Text = feedback.InsuranceCompanyName;
        sheet.Range[2, OfferedCol(index)].CellStyle.Font.Bold = true;
        sheet.Range[2, OfferedCol(index)].CellStyle.Font.Size = 13;
        sheet.Range[3, OfferedCol(index)].Text = "Offered";
        sheet.Range[3, ApCol(index)].Text = "AP";
        sheet.Range[3, CommentCol(index)].Text = "Comment";
        
        sheet.Range[3, SeparatorCol(index), 3, CommentCol(index)].CellStyle = headerStyle;
        sheet.Range[3, SeparatorCol(index)].ColumnWidth = 0.3;
        sheet.Range[3, SeparatorCol(index)].CellStyle.ColorIndex = ExcelKnownColors.Black;
    }

    public static void AddEnhancementsTable(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle headerStyle, IXlStyle normalCellStyle)
    {
        sheet.Range["A3"].Text = "Enhancement";
        sheet.Range["B3"].Text = "Description";
        sheet.Range["A3:B3"].CellStyle = headerStyle;
        var enhancements = feedback.Enhancements.ToImmutable();

        for (int i = 0; i < enhancements.Count; i++)
        {
            var enhancement = enhancements[i];
            sheet.Range[$"A{4 + i}"].Text = enhancement.Title;
            sheet.Range[$"B{4 + i}"].Text = enhancement.Description;
        }

        sheet.Range[$"A4:B{3 + enhancements.Count}"].CellStyle = normalCellStyle;
        sheet.Range[$"A3:B{3 + enhancements.Count}"].ColumnWidth = 30;
        sheet.Range[$"A3:B{3 + enhancements.Count}"].WrapText = true;
    }

    public static void AddPricingEnhancementsTable(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle headerStyle, IXlStyle normalCellStyle)
    {
        sheet.Range["L2"].Text = "Enhancements pricing breakdown.";
        sheet.Range["L2"].CellStyle.Font.Italic = true;
        sheet.Range["L3"].Text = "Enhancement";
        sheet.Range["M3"].Text = "AP";
        sheet.Range["L3:M3"].CellStyle = headerStyle;
        var enhancements = feedback.Enhancements.Where(e => e.AP > 0).ToImmutable();
        if (!enhancements.Any()) sheet.Range["L4"].Text = "No enhancements with AP selected";

        for (int i = 0; i < enhancements.Count; i++)
        {
            var enhancement = enhancements[i];
            sheet.Range[$"L{4 + i}"].Text = enhancement.Title;
            sheet.Range[$"M{4 + i}"].Text = $"{enhancement.AP * 100}%";
        }

        sheet.Range[$"L4:M{3 + enhancements.Count}"].CellStyle = normalCellStyle;
        sheet.Range[$"L3:M{3 + enhancements.Count}"].AutofitColumns();
    }
}