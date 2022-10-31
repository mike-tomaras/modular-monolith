using Incepted.Shared.DTOs;
using Incepted.Shared.ValueTypes;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;

namespace Incepted.DocGen.DocHelpers;

internal static class DocNbiHelpers
{
    public static void AddEnhancementListItem(IWParagraph paragraph, Enhancement enhancement)
    {
        var comment = enhancement.HasComment ? $" - Comment: {enhancement.Comment}" : string.Empty;
        paragraph.AppendText($"{enhancement.Title}{comment}");
        paragraph.ListFormat.ContinueListNumbering();
    }

    public static void AddEnhancementsList(WSection section, IEnumerable<Enhancement> enhancements)
    {
        var paragraph = section.AddParagraph();
        paragraph.ApplyStyle("Normal");
        paragraph.ListFormat.ApplyDefNumberedStyle();
        paragraph.ListFormat.RestartNumbering = true;

        AddEnhancementListItem(paragraph, enhancements.First());
        foreach (var enhancement in enhancements.Skip(1))
        {
            paragraph = section.AddParagraph();
            AddEnhancementListItem(paragraph, enhancement);
        }
    }
    public static void AddExclusionListItem(IWParagraph paragraph, Exclusion exclusion)
    {
        var comment = exclusion.HasComment ? $" - Comment: {exclusion.Comment}" : string.Empty;
        paragraph.AppendText($"{exclusion.Title}{comment}");
        paragraph.ListFormat.ContinueListNumbering();
    }

    public static void AddExclusionsList(WSection section, IEnumerable<Exclusion> exclusions)
    {
        var paragraph = section.AddParagraph();
        paragraph.ApplyStyle("Normal");
        paragraph.ListFormat.ApplyDefNumberedStyle();
        paragraph.ListFormat.RestartNumbering = true;

        AddExclusionListItem(paragraph, exclusions.First());
        foreach (var exclusion in exclusions.Skip(1))
        {
            paragraph = section.AddParagraph();
            AddExclusionListItem(paragraph, exclusion);
        }
    }

    public static IWParagraph AddFooter(WSection section)
    {
        var paragraph = section.HeadersFooters.Footer.AddParagraph();
        paragraph.ApplyStyle("Normal");
        paragraph.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
        var textRange = (WTextRange)paragraph.AppendText("Powered by Incepted");
        textRange.CharacterFormat.FontSize = 10f;
        textRange.CharacterFormat.Italic = true;

        return paragraph;
    }

    public static void AddHeader(string text, WSection section, int level = 2)
    {
        var paragraph = section.AddParagraph();
        paragraph.ApplyStyle($"Heading {level}");
        paragraph.AppendText(text);
    }

    public static void AddParagraph(string text, WSection section, bool isBold = false, bool isItalic = false)
    {
        var paragraph = section.AddParagraph();
        paragraph.ApplyStyle("Normal");
        var textRange = (WTextRange)paragraph.AppendText(text);
        textRange.CharacterFormat.Bold = isBold;
        textRange.CharacterFormat.Italic = isItalic;
    }

    public static void AddTopLevelHeader(SubmissionFeedbackDTO feedback, string insurerName, WSection section)
    {
        var paragraph = section.AddParagraph();
        paragraph.ApplyStyle("Heading 1");
        paragraph.AppendText($"NBI - {insurerName} - {feedback.Name}");
    }

    public static void SetStyles(WordDocument document)
    {
        //Create Paragraph styles
        WParagraphStyle style = (WParagraphStyle)document.AddParagraphStyle("Normal");
        style.CharacterFormat.FontName = "Calibri";
        style.CharacterFormat.FontSize = 11f;
        style.ParagraphFormat.BeforeSpacing = 0;
        style.ParagraphFormat.AfterSpacing = 8;
        style.ParagraphFormat.LineSpacing = 13.8f;

        style = (WParagraphStyle)document.AddParagraphStyle("Heading 1");
        style.ApplyBaseStyle("Normal");
        style.CharacterFormat.FontName = "Calibri Light";
        style.CharacterFormat.FontSize = 18f;
        style.CharacterFormat.TextColor = Syncfusion.Drawing.Color.FromArgb(0, 147, 221);
        style.ParagraphFormat.BeforeSpacing = 12;
        style.ParagraphFormat.AfterSpacing = 12;
        style.ParagraphFormat.Keep = true;
        style.ParagraphFormat.KeepFollow = true;
        style.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
        style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

        style = (WParagraphStyle)document.AddParagraphStyle("Heading 2");
        style.ApplyBaseStyle("Heading 1");
        style.CharacterFormat.FontSize = 16f;
        style.ParagraphFormat.BeforeSpacing = 10;
        style.ParagraphFormat.AfterSpacing = 10;
        style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;

        style = (WParagraphStyle)document.AddParagraphStyle("Heading 3");
        style.ApplyBaseStyle("Heading 2");
        style.CharacterFormat.FontSize = 14f;
    }
}