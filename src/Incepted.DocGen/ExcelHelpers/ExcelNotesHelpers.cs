using Incepted.Shared.DTOs;
using Syncfusion.XlsIO;
using IXlStyle = Syncfusion.XlsIO.IStyle;

namespace Incepted.DocGen.ExcelHelpers;

internal static class ExcelNotesHelpers
{
    private static int ColumnShift(int index) => index * 1;
    private static int InsurerCol(int index) => 1 + ColumnShift(index);

    public static void AddNotesTableValues(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle normalCellStyle, int index = 0)
    {
        sheet.Range[4, InsurerCol(index)].Text = feedback.Notes;
                
        sheet.Range[4, InsurerCol(index)].CellStyle = normalCellStyle;
        sheet.Range[4, InsurerCol(index)].ColumnWidth = 50;
        sheet.Range[4, InsurerCol(index)].WrapText = true;
    }

    public static void AddNotesTableValuesHeaders(SubmissionFeedbackDTO feedback, IWorksheet sheet, IXlStyle headerStyle, int index = 0)
    {
        sheet.Range[3, InsurerCol(index)].Text = feedback.InsuranceCompanyName;
        
        sheet.Range[3, InsurerCol(index)].CellStyle = headerStyle;
    }
}