using System.Windows.Media;

namespace CardAdjust.Models;

/// <summary>
/// 1文字ごとに解決済みのフォント設定
///
/// 個別調整(CharacterStyleOverride)を反映した後の、描画にそのまま使える状態を表す。
/// IsOverriddenは、個別調整による上書きが適用されているかを表す
/// (プレビュー画面での色分け表示にのみ使う)。
/// </summary>
public sealed record CharacterStyle(char Character, FontFamily FontFamily, double FontSize, double LetterSpacing, bool IsBold, bool IsOverridden);
