using System.IO;
using System.Text.Json;
using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// カードごとのレイアウト情報を読み書きするサービス
///
/// %AppData%\CardAdjust(_Debug)\card_layouts.json に、
/// カード画像の絶対パスをキーとした辞書で保存する。
/// </summary>
public class CardLayoutService
{
    private readonly string _layoutsPath;
    private readonly Dictionary<string, CardLayout> _layouts;

    public CardLayoutService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataPaths.FolderName);
        Directory.CreateDirectory(dir);
        _layoutsPath = Path.Combine(dir, "card_layouts.json");
        _layouts = TryLoad() ?? [];
    }

    public CardLayout? TryGet(string filePath) => _layouts.GetValueOrDefault(filePath);

    public void Save(string filePath, CardLayout layout)
    {
        _layouts[filePath] = layout;
        File.WriteAllText(_layoutsPath, JsonSerializer.Serialize(_layouts));
    }

    private Dictionary<string, CardLayout>? TryLoad()
    {
        if (!File.Exists(_layoutsPath))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, CardLayout>>(File.ReadAllText(_layoutsPath));
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
