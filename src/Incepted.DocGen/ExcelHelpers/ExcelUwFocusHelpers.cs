using Incepted.Shared;
using Incepted.Shared.DTOs;
using Syncfusion.XlsIO;
using IXlStyle = Syncfusion.XlsIO.IStyle;

namespace Incepted.DocGen.ExcelHelpers;

internal static class ExcelUwFocusHelpers
{
    private static int ColumnShift(int index) => index * 1;
    private static int InsurerCol(int index) => 2 + ColumnShift(index);

    public static void AddUwFocusTableValues(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle normalCellStyle, int index = 0)
    {
        var uwFocuses = feedback.UwFocus;
        var startRow = 4;
        var lastRow = startRow + uwFocuses.Count() - 1;

        for (int i = 0; i < uwFocuses.Count(); i++)
        {
            var uwFocus = uwFocuses[i];
            IRange uwFocusPosition = sheet.FindFirst(uwFocus, ExcelFindType.Text);
            
            sheet.Range[uwFocusPosition.Row, InsurerCol(index)].Text = "YES";
        }
        
        sheet.Range[startRow, InsurerCol(index), lastRow, InsurerCol(index)].CellStyle = normalCellStyle;
        sheet.Range[startRow - 1, InsurerCol(index), lastRow, InsurerCol(index)].AutofitColumns();//include header        
    }

    public static void AddUwFocusTableValuesHeaders(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle headerStyle, int index = 0)
    {
        sheet.Range[3, InsurerCol(index)].Text = feedback.InsuranceCompanyName;
        
        sheet.Range[3, InsurerCol(index)].CellStyle = headerStyle;
        sheet.Range[3, InsurerCol(index)].AutofitColumns();
    }

    public static void AddUwFocusTable(IEnumerable<SubmissionFeedbackDTO> feedbacks, IWorksheet sheet, IXlStyle headerStyle, IXlStyle normalCellStyle)
    {
        sheet.Range["A3"].Text = "UW focus";
        sheet.Range["A3"].CellStyle = headerStyle;
        
        var uwFocuses = feedbacks.SelectMany(f => f.UwFocus).Distinct().ToImmutable();

        for (int i = 0; i < uwFocuses.Count; i++)
        {
            sheet.Range[$"A{4 + i}"].Text = uwFocuses[i];
        }

        sheet.Range[$"A4:B{3 + uwFocuses.Count}"].CellStyle = normalCellStyle;
        sheet.Range[$"A3:B{3 + uwFocuses.Count}"].AutofitColumns();
    }
}