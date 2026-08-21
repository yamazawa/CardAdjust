namespace CardAdjust.Models;

/// <summary>
/// カードごとに個別設定された読取矩形(位置・サイズ)
///
/// カードに個別設定が無い場合はnullとして扱い、CardTemplateLayoutの全体設定を使う。
/// </summary>
public sealed record RegionOverride(double X, double Y, double Width, double Height);
