using System.Windows;

namespace CardAdjust.Models;

/// <summary>
/// 個別設定(CharacterStyleOverride)が実際に描画された範囲の外接矩形
///
/// プレビュー画面上に、番号付きのクリック可能な矩形を重ねて表示するために使う。
/// 番号(何番の個別設定か)はここでは持たず、MainViewModel側のOverrideSummaryと
/// Overrideの一致で対応付ける。
/// </summary>
public sealed record OverrideBounds(CharacterStyleOverride Override, Rect Bounds);
