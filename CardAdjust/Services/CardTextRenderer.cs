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

    // 個別設定の矩形(プレビュー専用、MainWindow側でクリック可能な要素として重ねて表示する)の余白。
    private const double AnnotationPadding = 2.0;

    /// <summary>
    /// 1行のテキストを、指定した矩形内に水平・垂直共に中央揃えで描画する
    ///
    /// stylesはtextと同じ長さ(1文字につき1要素)で渡す。
    /// overridesが実際に描画された範囲の外接矩形を返す(プレビュー画面での番号付き矩形表示に使う)。
    /// </summary>
    public IReadOnlyList<OverrideBounds> DrawCenteredSingleLine(DrawingContext context, string text, IReadOnlyList<CharacterStyle> styles,
        Rect rect, Brush foreground, IReadOnlyList<CharacterStyleOverride> overrides)
    {
        if (text.Length == 0)
            return [];

        var glyphs = text.Select((c, i) => CreateFormattedText(c.ToString(), TypefaceFor(styles[i]), styles[i].FontSize, foreground)).ToList();
        var spacingTotal = Enumerable.Range(0, glyphs.Count - 1).Sum(i => styles[i].LetterSpacing);
        var totalWidth = glyphs.Sum(g => g.Width) + spacingTotal;
        var lineHeight = glyphs.Max(g => g.Height);

        var x = rect.X + (rect.Width - totalWidth) / 2;
        var top = rect.Y + (rect.Height - lineHeight) / 2;

        // フォントサイズが文字ごとに異なっても下端が揃うよう、行の下端を基準に配置する。
        var charBounds = NewCharBoundsArray(text.Length);
        for (var i = 0; i < glyphs.Count; i++)
        {
            var y = top + (lineHeight - glyphs[i].Height);
            context.DrawText(glyphs[i], new Point(x, y));
            charBounds[i] = new Rect(new Point(x, y), new Size(glyphs[i].Width, glyphs[i].Height));
            x += glyphs[i].Width + styles[i].LetterSpacing;
        }

        return ComputeOverrideBounds(overrides, charBounds);
    }

    /// <summary>
    /// 複数行のテキストを、指定した矩形内に水平は左揃え、垂直はブロック全体で中央揃えして描画する
    ///
    /// 改行(\n)は描画せず、行の区切りとしてのみ扱う。stylesはtextと同じ長さで渡す
    /// (改行文字の位置にも要素は必要だが、その値は描画に使われない)。
    /// ある行が「---」そのものである場合、その行はテキストではなく水平線として描画する
    /// (太さ・上下MarginはdefaultDividerStyle、または一致するdividerOverridesの値を使う)。
    /// overridesが実際に描画された範囲の外接矩形を返す(プレビュー画面での番号付き矩形表示に使う)。
    /// </summary>
    public IReadOnlyList<OverrideBounds> DrawLeftAlignedMultiLine(DrawingContext context, string text, IReadOnlyList<CharacterStyle> styles,
        Rect rect, double lineSpacing, Brush foreground, IReadOnlyList<CharacterStyleOverride> overrides,
        DividerStyle defaultDividerStyle, IReadOnlyList<DividerStyleOverride> dividerOverrides)
    {
        if (text.Length == 0)
            return [];

        var lines = SplitIntoLines(text, styles);
        var dividers = lines
            .Select(line => line.RawText == DividerLineText
                ? DividerStyleBuilder.Resolve(line.StartIndex, line.RawText.Length, defaultDividerStyle, dividerOverrides)
                : null)
            .ToList();
        var lineHeights = ComputeLineHeights(lines, dividers, foreground);
        var totalHeight = lineHeights.Sum() + lineSpacing * Math.Max(0, lines.Count - 1);

        var charBounds = NewCharBoundsArray(text.Length);
        var y = rect.Y + (rect.Height - totalHeight) / 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (dividers[i] is { } divider)
                DrawDivider(context, rect, y + divider.MarginTop + divider.Thickness / 2, divider.Thickness, foreground);
            else
                DrawLine(context, lines[i].Styles, rect.X, y, lineHeights[i], foreground, charBounds, lines[i].StartIndex);

            y += lineHeights[i] + lineSpacing;
        }

        return ComputeOverrideBounds(overrides, charBounds);
    }

    // 水平線の行は太さ+上下Marginを行の高さとして扱い、それ以外の行は文字の高さの最大値を使う。
    private static List<double> ComputeLineHeights(List<(string RawText, List<CharacterStyle> Styles, int StartIndex)> lines,
        List<DividerStyle?> dividers, Brush foreground) =>
        lines.Select((line, i) => dividers[i] is { } divider
                ? divider.Thickness + divider.MarginTop + divider.MarginBottom
                : (line.Styles.Count == 0 ? 0 : line.Styles.Max(c => CreateFormattedText(c.Character.ToString(), TypefaceFor(c), c.FontSize, foreground).Height)))
            .ToList();

    // フォントサイズが文字ごとに異なっても下端が揃うよう、行の下端(lineTop + lineHeight)を基準に配置する。
    // 描画した各文字の矩形をcharBounds[startIndex + 行内位置]へ記録する(個別設定の矩形計算に使う)。
    private static void DrawLine(DrawingContext context, IReadOnlyList<CharacterStyle> lineStyles, double startX, double lineTop,
        double lineHeight, Brush foreground, Rect[] charBounds, int startIndex)
    {
        var x = startX;
        for (var j = 0; j < lineStyles.Count; j++)
        {
            var style = lineStyles[j];
            var glyph = CreateFormattedText(style.Character.ToString(), TypefaceFor(style), style.FontSize, foreground);
            var y = lineTop + (lineHeight - glyph.Height);
            context.DrawText(glyph, new Point(x, y));
            charBounds[startIndex + j] = new Rect(new Point(x, y), new Size(glyph.Width, glyph.Height));
            x += glyph.Width + style.LetterSpacing;
        }
    }

    private static void DrawDivider(DrawingContext context, Rect rect, double y, double thickness, Brush foreground) =>
        context.DrawLine(new Pen(foreground, thickness), new Point(rect.X, y), new Point(rect.X + rect.Width, y));

    private static Rect[] NewCharBoundsArray(int length)
    {
        var charBounds = new Rect[length];
        Array.Fill(charBounds, Rect.Empty);
        return charBounds;
    }

    // overrideごとに、実際に描画された文字の外接矩形を求める(番号付けはMainViewModel側で行う)。
    private static List<OverrideBounds> ComputeOverrideBounds(IReadOnlyList<CharacterStyleOverride> overrides, Rect[] charBounds)
    {
        var result = new List<OverrideBounds>();
        foreach (var o in overrides)
        {
            var indices = Enumerable.Range(Math.Max(0, o.Start), Math.Max(0, Math.Min(o.Length, charBounds.Length - Math.Max(0, o.Start))));
            var rects = indices.Select(i => charBounds[i]).Where(r => !r.IsEmpty).ToList();
            if (rects.Count == 0)
                continue;

            var bounds = rects.Aggregate(Rect.Union);
            bounds.Inflate(AnnotationPadding, AnnotationPadding);
            result.Add(new OverrideBounds(o, bounds));
        }

        return result;
    }

    private static List<(string RawText, List<CharacterStyle> Styles, int StartIndex)> SplitIntoLines(string text, IReadOnlyList<CharacterStyle> styles)
    {
        var lines = new List<(string, List<CharacterStyle>, int)>();
        var currentText = string.Empty;
        var currentStyles = new List<CharacterStyle>();
        var currentStart = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                lines.Add((currentText, currentStyles, currentStart));
                currentText = string.Empty;
                currentStyles = [];
                currentStart = i + 1;
                continue;
            }

            currentText += text[i];
            currentStyles.Add(styles[i]);
        }

        lines.Add((currentText, currentStyles, currentStart));
        return lines;
    }

    private static Typeface TypefaceFor(CharacterStyle style) =>
        new(style.FontFamily, FontStyles.Normal, style.IsBold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal);

    private static FormattedText CreateFormattedText(string text, Typeface typeface, double fontSize, Brush foreground) =>
        new(text, CultureInfo.GetCultureInfo("ja-JP"), FlowDirection.LeftToRight, typeface, fontSize, foreground, 1.0);
}
