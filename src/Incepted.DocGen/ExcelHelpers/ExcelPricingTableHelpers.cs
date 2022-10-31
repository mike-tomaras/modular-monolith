using Incepted.Shared;
using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using Syncfusion.XlsIO;
using System.Collections.Immutable;
using IXlStyle = Syncfusion.XlsIO.IStyle;

namespace Incepted.DocGen.ExcelHelpers;

internal static class ExcelPricingTableHelpers
{
    private static int ColumnShift(int index) => index * 8;
    private static int SeparatorCol(int index) => 3 + ColumnShift(index);
    private static int DeminimisCol(int index) => 4 + ColumnShift(index);
    private static int PremiumCol(int index) => 5 + ColumnShift(index);
    private static int RolCol(int index) => 6 + ColumnShift(index);
    private static int EnhancementCol(int index) => 7 + ColumnShift(index);
    private static int UwFeeCol(int index) => 8 + ColumnShift(index);
    private static int TotalCol(int index) => 9 + ColumnShift(index);
    private static int BreakFeeCol(int index) => 10 + ColumnShift(index);

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
            var enhancement = feedback.Enhancements[i];
            sheet.Range[$"L{4 + i}"].Text = enhancement.Title;
            sheet.Range[$"M{4 + i}"].Text = $"{enhancement.AP * 100}%";
        }

        var lastRow = enhancements.Any() ? 3 + enhancements.Count : 4;
        sheet.Range[$"L4:M{lastRow}"].CellStyle = normalCellStyle;
        sheet.Range[$"L3:M{lastRow}"].AutofitColumns();
    }

    public static void AddPricingTableOptions(SubmissionFeedbackDTO feedback, Money ev, IWorksheet sheet, IXlStyle normalCellStyle)
    {
        var limits = ImmutableList.CreateRange(feedback.Pricing.Limits);
        var optionCount = 0;
        for (int i = 0; i < limits.Count(); i++)
        {
            var limit = limits[i];
            var options = ImmutableList.CreateRange(feedback.Pricing.OptionsOfLimit(limit.Id));
            optionCount = options.Count();

            var startRow = 4 + options.Count * i;
            var range = $"A{startRow}:A{startRow + options.Count - 1}";
            sheet.Range[range].Merge();
            sheet.Range[range].Value2 = limit.ToAmount(ev);

            for (int j = 0; j < options.Count(); j++)
            {
                var rowNumber = startRow + j;
                var option = options[j];
                sheet.Range[$"B{rowNumber}"].Text = option.Retention.ToString();
            }
        }

        var lastRow = 3 + limits.Count * optionCount;
        sheet.Range[$"A4:B{lastRow}"].CellStyle = normalCellStyle;
        sheet.Range[$"A3:B{lastRow}"].AutofitColumns();//include header
    }

    public static void AddPricingTableValues(SubmissionFeedbackDTO feedback, Money ev, IWorksheet sheet, IXlStyle normalCellStyle, int index = 0)
    {
        var limits = ImmutableList.CreateRange(feedback.Pricing.Limits);
        var optionCount = 0;

        var startRow = 4;
        for (int i = 0; i < limits.Count(); i++)
        {
            var limit = limits[i];
            var options = ImmutableList.CreateRange(feedback.Pricing.OptionsOfLimit(limit.Id));
            optionCount = options.Count();

            var startRowForOption = startRow + options.Count * i;

            for (int j = 0; j < options.Count(); j++)
            {
                var rowNumber = startRowForOption + j;
                var option = options[j];
                sheet.Range[rowNumber, PremiumCol(index)].Text = option.Premium.ToAmountString();
                sheet.Range[rowNumber, RolCol(index)].Text = PricingCalc.RoLString(feedback.Pricing.EnterpriseValue, option.Premium, limit);
                sheet.Range[rowNumber, EnhancementCol(index)].Text = PricingCalc.EnhancementValueString(option.Premium, feedback.Enhancements);
                sheet.Range[rowNumber, TotalCol(index)].Text = PricingCalc.TotalString(option.Premium, feedback.Enhancements, feedback.Pricing.UwFee);
            }
        }
        var lastRow = 3 + limits.Count * optionCount;

        sheet.Range[startRow, DeminimisCol(index), lastRow, DeminimisCol(index)].Merge();
        sheet.Range[startRow, DeminimisCol(index), lastRow, DeminimisCol(index)].Value2 = feedback.Pricing.DeMinimis.Amount;
        
        sheet.Range[startRow, UwFeeCol(index), lastRow, UwFeeCol(index)].Merge();
        sheet.Range[startRow, UwFeeCol(index), lastRow, UwFeeCol(index)].Value2 = feedback.Pricing.UwFee.Amount;

        sheet.Range[startRow, BreakFeeCol(index), lastRow, BreakFeeCol(index)].Merge();
        sheet.Range[startRow, BreakFeeCol(index), lastRow, BreakFeeCol(index)].Value2 = feedback.Pricing.BreakFee.Amount;

        sheet.Range[lastRow + 1, UwFeeCol(index)].WrapText = true;
        sheet.Range[lastRow + 1, BreakFeeCol(index)].WrapText = true;
        if (feedback.Pricing.UwFeeWaiveOption) sheet.Range[lastRow + 1, UwFeeCol(index)].Text = "* Waived if deal is bound";
        if (feedback.Pricing.BreakFeeWaiveOption) sheet.Range[lastRow + 1, BreakFeeCol(index)].Text = "† Waived if exclusivity is granted";

        sheet.Range[startRow, SeparatorCol(index), lastRow, BreakFeeCol(index)].CellStyle = normalCellStyle;
        sheet.Range[startRow - 1, SeparatorCol(index), lastRow, BreakFeeCol(index)].AutofitColumns();//include header

        sheet.Range[startRow, SeparatorCol(index), lastRow, SeparatorCol(index)].CellStyle.ColorIndex = ExcelKnownColors.Black;
    }

    public static void AddPricingTableValuesHeaders(SubmissionFeedbackDTO feedback, Money ev, IWorksheet sheet, IXlStyle headerStyle, int index = 0)
    {
        sheet.Range[2, DeminimisCol(index)].Text = feedback.InsuranceCompanyName;
        sheet.Range[2, DeminimisCol(index)].CellStyle.Font.Bold = true;
        sheet.Range[2, DeminimisCol(index)].CellStyle.Font.Size = 13;
        sheet.Range[3, DeminimisCol(index)].Text = "De minimis";
        sheet.Range[3, PremiumCol(index)].Text = "Premium";
        sheet.Range[3, RolCol(index)].Text = "RoL";
        sheet.Range[3, EnhancementCol(index)].Text = "Enhancements";
        sheet.Range[3, UwFeeCol(index)].Text = feedback.Pricing.UwFeeWaiveOption ? "UW fee *" : "UW fee";
        sheet.Range[3, TotalCol(index)].Text = "Total cost";
        sheet.Range[3, BreakFeeCol(index)].Text = feedback.Pricing.BreakFeeWaiveOption ? "Break fee †" : "Break fee";
        sheet.Range[3, SeparatorCol(index), 3, BreakFeeCol(index)].CellStyle = headerStyle;
        sheet.Range[3, SeparatorCol(index)].ColumnWidth = 0.3;
        sheet.Range[3, SeparatorCol(index)].CellStyle.ColorIndex = ExcelKnownColors.Black;
    }

    public static void AddPricingTableOptionHeaders(SubmissionFeedbackDTO feedback, Money ev, IWorksheet sheet, IXlStyle headerStyle)
    {
        sheet.Range["A1"].Text = $"All amounts in the below table are in {ev.Currency} and net of any applicable taxes, if any.";
        sheet.Range["A1"].CellStyle.Font.Italic = true;
        sheet.Range["A3"].Text = "Limit of Liability";
        sheet.Range["B3"].Text = "Retention (%)";
        sheet.Range["A3:B3"].CellStyle = headerStyle;
    }
}