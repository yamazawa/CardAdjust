using System.IO;

namespace CardAdjust.Services;

/// <summary>
/// カードセットごとの読み込み対象フォルダパスをiniファイルから読み込むサービス
///
/// %AppData%\CardAdjust(_Debug)\config.ini に保存する。settings.jsonとは別ファイルで管理する。
/// キーは"CardFolder_{カードセットId}"の形式。
/// </summary>
public class CardFolderConfigService
{
    private const string KeyPrefix = "CardFolder_";
    private readonly string _configPath;

    public CardFolderConfigService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataPaths.FolderName);
        Directory.CreateDirectory(dir);
        _configPath = Path.Combine(dir, "config.ini");
    }

    /// <summary>
    /// カードセットIdごとのフォルダパスを読み込む
    ///
    /// ファイルが無い/値が無い場合は、defaultFoldersで新規作成して返す。
    /// </summary>
    public IReadOnlyDictionary<string, string> LoadOrCreateDefault(IReadOnlyDictionary<string, string> defaultFolders)
    {
        if (TryLoad() is { } loaded)
            return loaded;

        var lines = defaultFolders.Select(kv => $"{KeyPrefix}{kv.Key}={kv.Value}");
        File.WriteAllLines(_configPath, lines);
        return defaultFolders;
    }

    private Dictionary<string, string>? TryLoad()
    {
        if (!File.Exists(_configPath))
            return null;

        var result = new Dictionary<string, string>();
        foreach (var line in File.ReadAllLines(_configPath))
        {
            var separatorIndex = line.IndexOf('=');
            if (separatorIndex < 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            if (key.StartsWith(KeyPrefix) && value.Length > 0)
                result[key[KeyPrefix.Length..]] = value;
        }

        return result.Count > 0 ? result : null;
    }
}
