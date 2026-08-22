namespace CardAdjust.Models;

/// <summary>
/// 説明文の水平線(「---」の行)の見た目
/// </summary>
public sealed record DividerStyle(double Thickness, double MarginTop, double MarginBottom)
{
    public static DividerStyle Default { get; } = new(2.0, 8.0, 8.0);
}
