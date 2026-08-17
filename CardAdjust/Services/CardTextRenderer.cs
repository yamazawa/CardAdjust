using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CardAdjust.Services;

/// <summary>
/// タイトル・説明文を、1文字ずつ間隔を指定して描画するサービス
///
/// WPF標準のテキスト描画は文字間隔(文字ごとの追加スペース)を
/// サポートしないため、1文字ずつFormattedTextを作って個別に配置する。
/// </summary>
public class CardTextRenderer
{
    /// <summary>
    /// 1行のテキストを、指定した矩形内に水平・垂直共に中央揃えで描画する
    /// </summary>
    public void DrawCenteredSingleLine(DrawingContext context, string text, Rect rect,
        FontFamily fontFamily, double fontSize, double letterSpacing, Brush foreground)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var glyphs = text.Select(c => CreateFormattedText(c.ToString(), typeface, fontSize, foreground)).ToList();

        var totalWidth = glyphs.Sum(g => g.Width) + letterSpacing * Math.Max(0, glyphs.Count - 1);
        var lineHeight = glyphs.Max(g => g.Height);

        var x = rect.X + (rect.Width - totalWidth) / 2;
        var y = rect.Y + (rect.Height - lineHeight) / 2;

        foreach (var glyph in glyphs)
        {
            context.DrawText(glyph, new Point(x, y));
            x += glyph.Width + letterSpacing;
        }
    }

    /// <summary>
    /// 複数行のテキストを、指定した矩形内に水平は左揃え、垂直はブロック全体で中央揃えして描画する
    /// </summary>
    public void DrawLeftAlignedMultiLine(DrawingContext context, string text, Rect rect,
        FontFamily fontFamily, double fontSize, double letterSpacing, double lineSpacing, Brush foreground)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var lines = text.Split('\n');
        var lineHeight = CreateFormattedText("あ", typeface, fontSize, foreground).Height;
        var totalHeight = lineHeight * lines.Length + lineSpacing * Math.Max(0, lines.Length - 1);

        var y = rect.Y + (rect.Height - totalHeight) / 2;
        foreach (var line in lines)
        {
            var x = rect.X;
            foreach (var c in line)
            {
                var glyph = CreateFormattedText(c.ToString(), typeface, fontSize, foreground);
                context.DrawText(glyph, new Point(x, y));
                x += glyph.Width + letterSpacing;
            }

            y += lineHeight + lineSpacing;
        }
    }

    private static FormattedText CreateFormattedText(string text, Typeface typeface, double fontSize, Brush foreground) =>
        new(text, CultureInfo.GetCultureInfo("ja-JP"), FlowDirection.LeftToRight, typeface, fontSize, foreground, 1.0);
}
