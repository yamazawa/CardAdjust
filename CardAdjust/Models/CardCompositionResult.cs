using System.Windows.Media.Imaging;

namespace CardAdjust.Models;

/// <summary>
/// CardCompositionService.Composeの結果
///
/// TitleBounds/DescriptionBoundsは、プレビュー画面上に番号付きのクリック可能な矩形を
/// 重ねて表示するために使う(保存・一斉出力では使わない)。
/// </summary>
public sealed record CardCompositionResult(BitmapSource Image, IReadOnlyList<OverrideBounds> TitleBounds, IReadOnlyList<OverrideBounds> DescriptionBounds);
