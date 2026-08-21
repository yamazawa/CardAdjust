namespace CardAdjust.Models;

/// <summary>
/// 1枚のカードごとに保存する編集内容
///
/// 統一レイアウト設定(フォント種類・サイズ等)はアプリ全体設定側で持つため含まない。
/// 読取矩形(TitleRegion等)・貼付先矩形(TitleDestRegion等)は、
/// ドラッグ・リサイズで個別設定された場合のみ値を持つ。
/// nullの場合はCardTemplateLayoutの全体設定(固定座標)を使う。
/// </summary>
public sealed record CardLayout(
    RegionOverride? TitleRegion,
    RegionOverride? IllustrationRegion,
    RegionOverride? DescriptionRegion,
    RegionOverride? TitleDestRegion,
    RegionOverride? IllustrationDestRegion,
    RegionOverride? DescriptionDestRegion,
    string TitleText,
    string DescriptionText,
    bool KeepIllustrationAspectRatio,
    List<CharacterStyleOverride> TitleOverrides,
    List<CharacterStyleOverride> DescriptionOverrides);
