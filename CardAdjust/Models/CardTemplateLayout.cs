using System.Windows;

namespace CardAdjust.Models;

/// <summary>
/// 外枠テンプレート上に固定定義する②③④の表示矩形
///
/// テンプレート画像のピクセル座標系(1055×1490)で定義する。
/// カードごとの実際の描画位置は、タスク7のドラッグ調整で上書きできるようにする。
/// </summary>
public static class CardTemplateLayout
{
    public const double TemplateWidth = 1055;
    public const double TemplateHeight = 1490;

    public static readonly Rect TitleRect = new(60, 45, 935, 160);
    public static readonly Rect IllustrationRect = new(55, 245, 945, 865);
    public static readonly Rect DescriptionRect = new(70, 1135, 915, 295);
}
