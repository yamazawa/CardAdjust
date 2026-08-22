using System.Windows;

namespace CardAdjust.Models;

/// <summary>
/// プレビュー画面上に表示する、番号付きでクリック可能な個別設定の矩形
///
/// Numberは個別設定一覧(OverrideSummary)と対応する番号。
/// </summary>
public sealed record OverrideAnnotation(int Number, Rect Bounds, CharacterStyleOverride Override);
