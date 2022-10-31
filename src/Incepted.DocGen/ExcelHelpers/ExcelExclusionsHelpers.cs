using Incepted.Shared;
using Incepted.Shared.DTOs;
using Syncfusion.XlsIO;
using IXlStyle = Syncfusion.XlsIO.IStyle;

namespace Incepted.DocGen.ExcelHelpers;

internal static class ExcelExclusionsHelpers
{
    private static int ColumnShift(int index) => index * 3;
    private static int SeparatorCol(int index) => 3 + ColumnShift(index);
    private static int RequiredCol(int index) => 4 + ColumnShift(index);
    private static int CommentCol(int index) => 5 + ColumnShift(index);

    public static void AddExclusionsTableValues(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle normalCellStyle, int index = 0)
    {
        sheet.Range[3, CommentCol(index)].Text = 
            feedback.ExcludedCountries
            .Aggregate(string.Empty, (current, next) => current = current + ", " + next)
            .Trim()
            .Trim(',');

        var exclusions = feedback.Exclusions;
        var startRow = 6;
        var lastRow = startRow + exclusions.Count() - 1;

        for (int i = 0; i < exclusions.Count(); i++)
        {
            var exclusion = exclusions[i];
            IRange exclusionPosition = sheet.FindFirst(exclusion.Title, ExcelFindType.Text);
            
            sheet.Range[exclusionPosition.Row, RequiredCol(index)].Text = exclusion.InsurerRequiresIt ? "YES" : "NO";
            sheet.Range[exclusionPosition.Row, CommentCol(index)].Text = exclusion.Comment;            
        }
        
        sheet.Range[startRow, SeparatorCol(index), lastRow, CommentCol(index)].CellStyle = normalCellStyle;
        for (int i = 0; i < exclusions.Count(); i++)
        {
            var exclusion = exclusions[i];
            IRange exclusionPosition = sheet.FindFirst(exclusion.Title, ExcelFindType.Text);

            sheet.Range[exclusionPosition.Row, RequiredCol(index)].CellStyle.ColorIndex = exclusion.InsurerRequiresIt ? ExcelKnownColors.Light_green : ExcelKnownColors.White;
        }
        sheet.Range[startRow - 1, SeparatorCol(index), lastRow, RequiredCol(index)].AutofitColumns();//include header
        sheet.Range[startRow, CommentCol(index), lastRow, CommentCol(index)].ColumnWidth = 30;
        sheet.Range[startRow, CommentCol(index), lastRow, CommentCol(index)].WrapText = true;
        sheet.Range[startRow, SeparatorCol(index), lastRow, SeparatorCol(index)].CellStyle.ColorIndex = ExcelKnownColors.Black;
    }

    public static void AddExclusionsTableValuesHeaders(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle headerStyle, int index = 0)
    {
        sheet.Range[2, RequiredCol(index)].Text = feedback.InsuranceCompanyName;
        sheet.Range[2, RequiredCol(index)].CellStyle.Font.Bold = true;
        sheet.Range[2, RequiredCol(index)].CellStyle.Font.Size = 13;
        sheet.Range[5, RequiredCol(index)].Text = "Required";
        sheet.Range[5, CommentCol(index)].Text = "Comment";
        
        sheet.Range[5, SeparatorCol(index), 5, CommentCol(index)].CellStyle = headerStyle;
        sheet.Range[5, SeparatorCol(index)].ColumnWidth = 0.3;
        sheet.Range[5, SeparatorCol(index)].CellStyle.ColorIndex = ExcelKnownColors.Black;
    }

    public static void AddExclusionsTable(IEnumerable<SubmissionFeedbackDTO> feedbacks, IWorksheet sheet, IXlStyle headerStyle, IXlStyle normalCellStyle)
    {
        sheet.Range["A3"].Text = "Excluded countries";
        sheet.Range["A3:B3"].CellStyle = headerStyle;
        sheet.Range[3, 3, 3, 2 + ColumnShift(feedbacks.Count())].CellStyle = normalCellStyle;
        sheet.Range["A5"].Text = "Title";
        sheet.Range["B5"].Text = "Description";
        sheet.Range["A5:B5"].CellStyle = headerStyle;
        
        var exclusions = feedbacks.SelectMany(f => f.Exclusions).DistinctBy(e => e.Title).ToImmutable();

        for (int i = 0; i < exclusions.Count; i++)
        {
            var exclusion = exclusions[i];
            sheet.Range[$"A{6 + i}"].Text = exclusion.Title;
            sheet.Range[$"B{6 + i}"].Text = exclusion.Description;
        }

        sheet.Range[$"A6:B{5 + exclusions.Count}"].CellStyle = normalCellStyle;
        sheet.Range[$"A5:B{5 + exclusions.Count}"].ColumnWidth = 30;
        sheet.Range[$"A5:B{5 + exclusions.Count}"].WrapText = true;
    }
}