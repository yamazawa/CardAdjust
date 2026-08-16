using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CardAdjust.Models;
using CardAdjust.Services;

namespace CardAdjust.ViewModels;

/// <summary>
/// メイン画面のViewModel
///
/// カード一覧の読み込み・選択と、選択中カードの元画像表示を担う。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly AppSettingsService _settingsService;
    private bool _settingsDirty;

    [ObservableProperty]
    private IReadOnlyList<CardImage> _cardList;

    [ObservableProperty]
    private CardImage? _selectedCard;

    // 選択中カードの元画像。プレビュー合成が実装されるまでは、
    // プレビュー欄にもこの画像をそのまま表示する。
    [ObservableProperty]
    private BitmapImage? _sourceImage;

    [ObservableProperty]
    private double _windowWidth;

    [ObservableProperty]
    private double _windowHeight;

    public MainViewModel(CardFolderService cardFolderService, AppSettingsService settingsService, AppSettings settings, string cardFolder)
    {
        _settingsService = settingsService;

        // 初期表示時点ではまだ設定変更ではないため、_settingsDirtyを立てないよう
        // プロパティではなくフィールドへ直接代入する。
        _windowWidth = settings.WindowWidth;
        _windowHeight = settings.WindowHeight;

        _cardList = cardFolderService.LoadCardList(cardFolder);
        SelectedCard = CardList.FirstOrDefault();
    }

    partial void OnSelectedCardChanged(CardImage? value) =>
        SourceImage = value is null ? null : LoadImage(value.FilePath);

    partial void OnWindowWidthChanged(double value) => MarkSettingsDirty();

    partial void OnWindowHeightChanged(double value) => MarkSettingsDirty();

    private void MarkSettingsDirty() => _settingsDirty = true;

    /// <summary>
    /// ウィンドウサイズに変更があれば設定を保存する
    /// </summary>
    public void SaveSettingsIfDirty()
    {
        if (!_settingsDirty)
            return;

        _settingsDirty = false;
        _settingsService.Save(new AppSettings(WindowWidth, WindowHeight));
    }

    private static BitmapImage LoadImage(string filePath)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(filePath);
        image.EndInit();
        image.Freeze();
        return image;
    }
}
