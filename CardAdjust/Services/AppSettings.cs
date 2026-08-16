namespace CardAdjust.Services;

/// <summary>
/// アプリ全体設定のシリアライズ用DTO
///
/// カードフォルダパスはconfig.ini(CardFolderConfigService)で別管理する。
/// </summary>
public sealed record AppSettings(
    double WindowWidth = Styles.LayoutConstants.WindowWidth,
    double WindowHeight = Styles.LayoutConstants.WindowHeight);
