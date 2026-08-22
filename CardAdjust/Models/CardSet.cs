using System.Windows.Media;

namespace CardAdjust.Models;

/// <summary>
/// フォルダ・テンプレート画像・文字色をひとまとめにしたカードセット
///
/// 例:使用カード(黒枠線テンプレート・白文字)、場のカード(白枠線テンプレート・黒文字)。
/// </summary>
public sealed record CardSet(string Id, string Name, string FolderPath, string TemplateImageUri, Brush TextForeground);
