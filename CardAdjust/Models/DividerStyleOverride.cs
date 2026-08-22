namespace CardAdjust.Models;

/// <summary>
/// 説明文中の1つの水平線(「---」の行、Start～Start+Length)に対する見た目の個別上書き
/// </summary>
public sealed record DividerStyleOverride(int Start, int Length, double Thickness, double MarginTop, double MarginBottom);
