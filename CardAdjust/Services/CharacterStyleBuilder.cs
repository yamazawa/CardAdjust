using System.Windows.Media;
using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// テキストの各文字に、個別上書き(CharacterStyleOverride)を反映したスタイルを割り当てる
/// </summary>
public static class CharacterStyleBuilder
{
    public static IReadOnlyList<CharacterStyle> Build(string text, FontFamily defaultFontFamily, double defaultFontSize,
        double defaultLetterSpacing, IReadOnlyList<CharacterStyleOverride> overrides)
    {
        var styles = new CharacterStyle[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            var matched = overrides.FirstOrDefault(o => i >= o.Start && i < o.Start + o.Length);
            styles[i] = matched is null
                ? new CharacterStyle(text[i], defaultFontFamily, defaultFontSize, defaultLetterSpacing)
                : new CharacterStyle(text[i], new FontFamily(matched.FontFamilyName), matched.FontSize, matched.LetterSpacing);
        }

        return styles;
    }
}
