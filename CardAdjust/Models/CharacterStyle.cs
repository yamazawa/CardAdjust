using System.Windows.Media;

namespace CardAdjust.Models;

/// <summary>
/// 1文字ごとに解決済みのフォント設定
///
/// 個別調整(CharacterStyleOverride)を反映した後の、描画にそのまま使える状態を表す。
/// </summary>
public sealed record CharacterStyle(char Character, FontFamily FontFamily, double FontSize, double LetterSpacing);
