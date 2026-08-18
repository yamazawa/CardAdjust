using System.Windows;

namespace CardAdjust.Styles;

/// <summary>
/// View全体で使うレイアウト関連の定数値
///
/// サイズ・余白を変更する際はここを直す。
/// </summary>
public static class LayoutConstants
{
    public const double WindowHeight = 700;
    public const double WindowWidth = 1100;
    public const double WindowMinHeight = 500;
    public const double WindowMinWidth = 800;

    public const double CardListWidth = 200;
    public const double ElementPanelWidth = 240;
    public const double DescriptionTextBoxHeight = 80;
    public const double RegionResizeHandleSize = 14;

    public static readonly Thickness RootMargin = new(12);
    public static readonly Thickness SectionSpacing = new(4);
    public static readonly Thickness FieldSpacing = new(0, 0, 0, 4);
    public static readonly Thickness ElementSpacing = new(0, 0, 0, 12);
    public static readonly Thickness ButtonSpacing = new(0, 4, 8, 0);
}
