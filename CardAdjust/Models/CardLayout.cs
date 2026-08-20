namespace CardAdjust.Models;

/// <summary>
/// 1枚のカードごとに保存する編集内容
///
/// 統一レイアウト設定(フォント種類・サイズ等)はアプリ全体設定側で持つため含まない。
/// </summary>
public sealed record CardLayout(
    double TitleRegionX, double TitleRegionY, double TitleRegionWidth, double TitleRegionHeight,
    double IllustrationRegionX, double IllustrationRegionY, double IllustrationRegionWidth, double IllustrationRegionHeight,
    double DescriptionRegionX, double DescriptionRegionY, double DescriptionRegionWidth, double DescriptionRegionHeight,
    string TitleText,
    string DescriptionText,
    bool KeepIllustrationAspectRatio,
    List<CharacterStyleOverride> TitleOverrides,
    List<CharacterStyleOverride> DescriptionOverrides);
