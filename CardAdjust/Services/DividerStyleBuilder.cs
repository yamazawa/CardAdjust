using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// 水平線の位置(Start・Length)に、個別上書き(DividerStyleOverride)を反映したスタイルを割り当てる
/// </summary>
public static class DividerStyleBuilder
{
    public static DividerStyle Resolve(int start, int length, DividerStyle defaultStyle, IReadOnlyList<DividerStyleOverride> overrides)
    {
        var matched = overrides.FirstOrDefault(o => o.Start == start && o.Length == length);
        return matched is null ? defaultStyle : new DividerStyle(matched.Thickness, matched.MarginTop, matched.MarginBottom);
    }
}
