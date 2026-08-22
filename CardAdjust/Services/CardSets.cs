using System.IO;
using System.Windows.Media;
using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// アプリで扱うカードセット(フォルダ・テンプレート画像・文字色の組)の既定値一覧
///
/// フォルダパスはconfig.ini(CardFolderConfigService)で上書きできる。
/// テンプレート画像・文字色はカードセットの見た目そのものなのでここに固定で定義する。
/// 新しいカードセットを追加する場合はここに定義を足す。
/// </summary>
public static class CardSets
{
    private static readonly string DesktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public static readonly IReadOnlyList<CardSet> All =
    [
        new CardSet("used", "使用カード",
            Path.Combine(DesktopFolder, "オークションポーカー", "使用カード"),
            "pack://application:,,,/image/黒枠線テンプレート.png", Brushes.White),
        new CardSet("field", "場のカード",
            Path.Combine(DesktopFolder, "オークションポーカー", "場のカード"),
            "pack://application:,,,/image/白枠線テンプレート.png", Brushes.Black),
    ];
}
