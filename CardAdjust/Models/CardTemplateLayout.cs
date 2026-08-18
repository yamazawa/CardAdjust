using System.Windows;

namespace CardAdjust.Models;

/// <summary>
/// 外枠テンプレート上に固定定義する②③④の表示矩形
///
/// テンプレート画像のピクセル座標系(1055×1490)で定義する。
/// カードごとの実際の読取位置は、タスク7のドラッグ調整で上書きできる
/// (この矩形は読取位置の初期値、および描画先の固定矩形として使う)。
/// </summary>
public static class CardTemplateLayout
{
    public const double TemplateWidth = 1055;
    public const double TemplateHeight = 1490;

    public const double TitleX = 60;
    public const double TitleY = 45;
    public const double TitleWidth = 935;
    public const double TitleHeight = 160;

    public const double IllustrationX = 55;
    public const double IllustrationY = 245;
    public const double IllustrationWidth = 945;
    public const double IllustrationHeight = 865;

    public const double DescriptionX = 70;
    public const double DescriptionY = 1135;
    public const double DescriptionWidth = 915;
    public const double DescriptionHeight = 295;

    public static readonly Rect TitleRect = new(TitleX, TitleY, TitleWidth, TitleHeight);
    public static readonly Rect IllustrationRect = new(IllustrationX, IllustrationY, IllustrationWidth, IllustrationHeight);
    public static readonly Rect DescriptionRect = new(DescriptionX, DescriptionY, DescriptionWidth, DescriptionHeight);
}
