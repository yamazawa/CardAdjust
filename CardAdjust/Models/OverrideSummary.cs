namespace CardAdjust.Models;

/// <summary>
/// 個別設定一覧に表示する1件分の情報
///
/// Numberはプレビュー画面上の番号付き矩形と対応する(開始位置順に採番)。
/// </summary>
public sealed record OverrideSummary(int Number, string Label, CharacterStyleOverride Override);
