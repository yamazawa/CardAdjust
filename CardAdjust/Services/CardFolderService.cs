using System.IO;
using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// 使用カードフォルダ内の画像ファイルを列挙するサービス
/// </summary>
public class CardFolderService
{
    private static readonly string[] SupportedExtensions = [".png", ".jpg", ".jpeg"];

    public IReadOnlyList<CardImage> LoadCardList(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return [];

        return Directory.EnumerateFiles(folderPath)
            .Where(path => SupportedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(path => new CardImage { FilePath = path })
            .ToList();
    }
}
