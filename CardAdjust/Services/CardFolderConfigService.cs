using System.IO;

namespace CardAdjust.Services;

/// <summary>
/// 読み込み対象の使用カードフォルダパスをiniファイルから読み込むサービス
///
/// %AppData%\CardAdjust(_Debug)\config.ini に保存する。settings.jsonとは別ファイルで管理する。
/// </summary>
public class CardFolderConfigService
{
    private const string CardFolderKey = "CardFolder";
    private readonly string _configPath;

    public CardFolderConfigService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataPaths.FolderName);
        Directory.CreateDirectory(dir);
        _configPath = Path.Combine(dir, "config.ini");
    }

    /// <summary>
    /// カードフォルダパスを読み込む
    ///
    /// ファイルが無い/値が無い場合は既定値で新規作成して返す。
    /// </summary>
    public string LoadOrCreateDefault(string defaultCardFolder)
    {
        if (TryLoad() is { } loaded)
            return loaded;

        File.WriteAllLines(_configPath, [$"{CardFolderKey}={defaultCardFolder}"]);
        return defaultCardFolder;
    }

    private string? TryLoad()
    {
        if (!File.Exists(_configPath))
            return null;

        foreach (var line in File.ReadAllLines(_configPath))
        {
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            if (key == CardFolderKey && value.Length > 0)
                return value;
        }

        return null;
    }
}
