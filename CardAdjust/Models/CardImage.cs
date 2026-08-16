using System.IO;

namespace CardAdjust.Models;

/// <summary>
/// 使用カードフォルダ内の1枚のカード画像ファイル
/// </summary>
public sealed class CardImage
{
    public required string FilePath { get; init; }

    public string DisplayName => Path.GetFileNameWithoutExtension(FilePath);
}
