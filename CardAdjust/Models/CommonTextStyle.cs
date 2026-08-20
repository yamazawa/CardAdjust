namespace CardAdjust.Models;

/// <summary>
/// タイトル・説明文の統一レイアウト設定(フォント種類・サイズ・文字間隔)
///
/// 文字区間ごとの個別上書きはカードごとに別途保持するため含まない。
/// </summary>
public sealed record CommonTextStyle(string FontFamilyName, double FontSize, double LetterSpacing, bool IsBold);
