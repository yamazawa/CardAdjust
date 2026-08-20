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

    // 個別調整による上書きの色分け表示・番号付き矩形(プレビュー専用)に使う色・サイズ。
    private static readonly Brush OverrideHighlightBrush = Brushes.Yellow;
    private const double AnnotationPadding = 2.0;
    private const double AnnotationNumberFontSize = 14.0;

    /// <summary>
    /// 1行のテキストを、指定した矩形内に水平・垂直共に中央揃えで描画する
    ///
    /// stylesはtextと同じ長さ(1文字につき1要素)で渡す。
    /// highlightOverridesがtrueの場合、個別調整で上書きされている文字を色分け表示し、
    /// overridesの区間ごとに番号付きの矩形を重ねて描画する。
    /// </summary>
    public void DrawCenteredSingleLine(DrawingContext context, string text, IReadOnlyList<CharacterStyle> styles, Rect rect,
        Brush foreground, bool highlightOverrides, IReadOnlyList<CharacterStyleOverride> overrides)
    {
        if (text.Length == 0)
            return;

        var glyphs = text.Select((c, i) => CreateFormattedText(c.ToString(), TypefaceFor(styles[i]), styles[i].FontSize,
            BrushFor(styles[i], foreground, highlightOverrides))).ToList();
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

        if (highlightOverrides)
            DrawOverrideAnnotations(context, overrides, charBounds);
    }

    /// <summary>
    /// 複数行のテキストを、指定した矩形内に水平は左揃え、垂直はブロック全体で中央揃えして描画する
    ///
    /// 改行(\n)は描画せず、行の区切りとしてのみ扱う。stylesはtextと同じ長さで渡す
    /// (改行文字の位置にも要素は必要だが、その値は描画に使われない)。
    /// ある行が「---」そのものである場合、その行はテキストではなく水平線として描画する。
    /// highlightOverridesがtrueの場合、個別調整で上書きされている文字を色分け表示し、
    /// overridesの区間ごとに番号付きの矩形を重ねて描画する。
    /// </summary>
    public void DrawLeftAlignedMultiLine(DrawingContext context, string text, IReadOnlyList<CharacterStyle> styles, Rect rect,
        double lineSpacing, Brush foreground, bool highlightOverrides, IReadOnlyList<CharacterStyleOverride> overrides)
    {
        if (text.Length == 0)
            return;

        var lines = SplitIntoLines(text, styles);
        var lineHeights = lines
            .Select(line => line.Styles.Count == 0 ? 0 : line.Styles.Max(c => CreateFormattedText(c.Character.ToString(), TypefaceFor(c), c.FontSize, foreground).Height))
            .ToList();
        var totalHeight = lineHeights.Sum() + lineSpacing * Math.Max(0, lines.Count - 1);

        var charBounds = NewCharBoundsArray(text.Length);
        var y = rect.Y + (rect.Height - totalHeight) / 2;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].RawText == DividerLineText)
                DrawDivider(context, rect, y + lineHeights[i] / 2, foreground);
            else
                DrawLine(context, lines[i].Styles, rect.X, y, lineHeights[i], foreground, highlightOverrides, charBounds, lines[i].StartIndex);

            y += lineHeights[i] + lineSpacing;
        }

        if (highlightOverrides)
            DrawOverrideAnnotations(context, overrides, charBounds);
    }

    // フォントサイズが文字ごとに異なっても下端が揃うよう、行の下端(lineTop + lineHeight)を基準に配置する。
    // 描画した各文字の矩形をcharBounds[startIndex + 行内位置]へ記録する(番号付き矩形の描画に使う)。
    private static void DrawLine(DrawingContext context, IReadOnlyList<CharacterStyle> lineStyles, double startX, double lineTop,
        double lineHeight, Brush foreground, bool highlightOverrides, Rect[] charBounds, int startIndex)
    {
        var x = startX;
        for (var j = 0; j < lineStyles.Count; j++)
        {
            var style = lineStyles[j];
            var glyph = CreateFormattedText(style.Character.ToString(), TypefaceFor(style), style.FontSize, BrushFor(style, foreground, highlightOverrides));
            var y = lineTop + (lineHeight - glyph.Height);
            context.DrawText(glyph, new Point(x, y));
            charBounds[startIndex + j] = new Rect(new Point(x, y), new Size(glyph.Width, glyph.Height));
            x += glyph.Width + style.LetterSpacing;
        }
    }

    private static void DrawDivider(DrawingContext context, Rect rect, double y, Brush foreground) =>
        context.DrawLine(new Pen(foreground, DividerThickness), new Point(rect.X, y), new Point(rect.X + rect.Width, y));

    private static Brush BrushFor(CharacterStyle style, Brush foreground, bool highlightOverrides) =>
        highlightOverrides && style.IsOverridden ? OverrideHighlightBrush : foreground;

    private static Rect[] NewCharBoundsArray(int length)
    {
        var charBounds = new Rect[length];
        Array.Fill(charBounds, Rect.Empty);
        return charBounds;
    }

    // overridesを開始位置順に並べ、①②③...に対応する番号を振って矩形と番号を描画する。
    // 番号は、プレビュー画面下の個別設定一覧(MainViewModelのOverrideSummary)と同じ順序で対応させる。
    private static void DrawOverrideAnnotations(DrawingContext context, IReadOnlyList<CharacterStyleOverride> overrides, Rect[] charBounds)
    {
        var ordered = overrides.OrderBy(o => o.Start).ToList();
        for (var i = 0; i < ordered.Count; i++)
            DrawOverrideAnnotation(context, i + 1, ordered[i], charBounds);
    }

    private static void DrawOverrideAnnotation(DrawingContext context, int number, CharacterStyleOverride o, Rect[] charBounds)
    {
        var indices = Enumerable.Range(Math.Max(0, o.Start), Math.Max(0, Math.Min(o.Length, charBounds.Length - Math.Max(0, o.Start))));
        var rects = indices.Select(i => charBounds[i]).Where(r => !r.IsEmpty).ToList();
        if (rects.Count == 0)
            return;

        var bounds = rects.Aggregate(Rect.Union);
        bounds.Inflate(AnnotationPadding, AnnotationPadding);
        context.DrawRectangle(null, new Pen(OverrideHighlightBrush, 1.5), bounds);

        var numberTypeface = new Typeface(new FontFamily("Yu Gothic UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        var numberText = CreateFormattedText(number.ToString(), numberTypeface, AnnotationNumberFontSize, OverrideHighlightBrush);
        context.DrawText(numberText, new Point(bounds.X, bounds.Y - numberText.Height));
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
