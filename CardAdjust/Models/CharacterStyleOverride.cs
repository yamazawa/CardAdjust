namespace CardAdjust.Models;

/// <summary>
/// テキストの一部区間(Start～Start+Length)に対するフォント設定の個別上書き
///
/// 個別調整ダイアログの「クリア」で、対象区間の上書きを削除して統一設定に戻す。
/// </summary>
public sealed record CharacterStyleOverride(int Start, int Length, string FontFamilyName, double FontSize, double LetterSpacing, bool IsBold);
