using Syncfusion.Drawing;
using Syncfusion.XlsIO;
using IXlStyle = Syncfusion.XlsIO.IStyle;

namespace Incepted.DocGen.ExcelHelpers;

internal static class ExcelStyilingHelpers
{
    public static IXlStyle CreateNormalCellStyle(IWorkbook workbook)
    {
        var normalCellStyle = workbook.Styles.Add("CellStyle");
        normalCellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        normalCellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        normalCellStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        normalCellStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
        normalCellStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
        normalCellStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
        return normalCellStyle;
    }

    public static IXlStyle CreateHeaderCellStyle(IWorkbook workbook)
    {
        var headerStyle = workbook.Styles.Add("HeaderStyle");
        headerStyle.Color = Color.LightGray;
        headerStyle.Font.Bold = true;
        headerStyle.Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
        headerStyle.Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
        headerStyle.Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
        headerStyle.Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
        headerStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        headerStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        return headerStyle;
    }
}