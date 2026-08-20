namespace CardAdjust.Services;

/// <summary>
/// アプリ全体設定のシリアライズ用DTO
///
/// カードフォルダパスはconfig.ini(CardFolderConfigService)で別管理する。
/// ②タイトル・④説明文の統一レイアウト設定は共有せず、別々に保持する。
/// </summary>
public sealed record AppSettings(
    double WindowWidth = Styles.LayoutConstants.WindowWidth,
    double WindowHeight = Styles.LayoutConstants.WindowHeight,
    string TitleFontFamily = "Yu Gothic UI",
    double TitleFontSize = 56,
    double TitleLetterSpacing = 6,
    bool TitleBold = false,
    string DescriptionFontFamily = "Yu Gothic UI",
    double DescriptionFontSize = 28,
    double DescriptionLetterSpacing = 2,
    double DescriptionLineSpacing = 10,
    bool DescriptionBold = false,
    string BatchExportFolder = "");
