using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// タイトル・説明文を、1文字ずつスタイル(フォント種類・サイズ・文字間隔)を指定して描画するサービス
///
/// WPF標準のテキスト描画は文字間隔(文字ごとの追加スペース)や文字単位のフォント切り替えを
/// サポートしないため、1文字ずつFormattedTextを作って個別に配置する。
/// </summary>
public class CardTextRenderer
{
    // 説明文のある行がこの文字列そのものである場合、その行は水平線として描画する。
    private const string DividerLineText = "---";
    private const double DividerThickness = 3.0;

    /// <summary>
    /// 1行のテキストを、指定した矩形内に水平・垂直共に中央揃えで描画する
    ///
    /// stylesはtextと同じ長さ(1文字につき1要素)で渡す。
    /// </summary>
    public void DrawCenteredSingleLine(DrawingContext context, string text, IReadOnlyList<CharacterStyle> styles, Rect rect, Brush foreground)
    {
        if (text.Length == 0)
            return;

        var glyphs = text.Select((c, i) => CreateFormattedText(c.ToString(), TypefaceFor(styles[i]), styles[i].FontSize, foreground)).ToList();
        var spacingTotal = Enumerable.Range(0, glyphs.Count - 1).Sum(i => styles[i].LetterSpacing);
        var totalWidth = glyphs.Sum(g => g.Width) + spacingTotal;
        var lineHeight = glyphs.Max(g => g.Height);

        var x = rect.X + (rect.Width - totalWidth) / 2;
        var top = rect.Y + (rect.Height - lineHeight) / 2;

        // フォントサイズが文字ごとに異なっても下端が揃うよう、行の下端を基準に配置する。
        for (var i = 0; i < glyphs.Count; i++)
        {
            context.DrawText(glyphs[i], new Point(x, top + (lineHeight - glyphs[i].Height)));
            x += glyphs[i].Width + styles[i].LetterSpacing;
        }
    }

    /// <summary>
    /// 複数行のテキストを、指定した矩形内に水平は左揃え、垂直はブロック全体で中央揃えして描画する
    ///
    /// 改行(\n)は描画せず、行の区切りとしてのみ扱う。stylesはtextと同じ長さで渡す
    /// (改行文字の位置にも要素は必要だが、その値は描画に使われない)。
    /// ある行が「---」そのものである場合、その行はテキストではなく水平線として描画する。
    /// </summary>
    public void DrawLeftAlignedMultiLine(DrawingContext context, string text, IReadOnlyList<CharacterStyle> styles, Rect rect,
        double lineSpacing, Brush foreground)
    {
        if (text.Length == 0)
            return;

        var lines = SplitIntoLines(text, styles);
        var lineHeights = lines
            .Select(line => line.Styles.Count == 0 ? 0 : line.Styles.Max(c => CreateFormattedText(c.Character.ToString(), TypefaceFor(c), c.FontSize, foreground).Height))
            .ToList();
        var totalHeight = lineHeights.Sum() + lineSpacing * Math.Max(0, lines.Count - 1);

        var y = rect.Y + (rect.Height - totalHeight) / 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].RawText == DividerLineText)
                DrawDivider(context, rect, y + lineHeights[i] / 2, foreground);
            else
                DrawLine(context, lines[i].Styles, rect.X, y, lineHeights[i], foreground);

            y += lineHeights[i] + lineSpacing;
        }
    }

    // フォントサイズが文字ごとに異なっても下端が揃うよう、行の下端(lineTop + lineHeight)を基準に配置する。
    private static void DrawLine(DrawingContext context, IReadOnlyList<CharacterStyle> lineStyles, double startX, double lineTop,
        double lineHeight, Brush foreground)
    {
        var x = startX;
        foreach (var style in lineStyles)
        {
            var glyph = CreateFormattedText(style.Character.ToString(), TypefaceFor(style), style.FontSize, foreground);
            context.DrawText(glyph, new Point(x, lineTop + (lineHeight - glyph.Height)));
            x += glyph.Width + style.LetterSpacing;
        }
    }

    private static void DrawDivider(DrawingContext context, Rect rect, double y, Brush foreground) =>
        context.DrawLine(new Pen(foreground, DividerThickness), new Point(rect.X, y), new Point(rect.X + rect.Width, y));

    private static List<(string RawText, List<CharacterStyle> Styles)> SplitIntoLines(string text, IReadOnlyList<CharacterStyle> styles)
    {
        var lines = new List<(string, List<CharacterStyle>)>();
        var currentText = string.Empty;
        var currentStyles = new List<CharacterStyle>();
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                lines.Add((currentText, currentStyles));
                currentText = string.Empty;
                currentStyles = [];
                continue;
            }

            currentText += text[i];
            currentStyles.Add(styles[i]);
        }

        lines.Add((currentText, currentStyles));
        return lines;
    }

    private static Typeface TypefaceFor(CharacterStyle style) =>
        new(style.FontFamily, FontStyles.Normal, style.IsBold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal);

    private static FormattedText CreateFormattedText(string text, Typeface typeface, double fontSize, Brush foreground) =>
        new(text, CultureInfo.GetCultureInfo("ja-JP"), FlowDirection.LeftToRight, typeface, fontSize, foreground, 1.0);
}
