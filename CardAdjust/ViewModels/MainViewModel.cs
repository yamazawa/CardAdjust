using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CardAdjust.Models;
using CardAdjust.Services;

namespace CardAdjust.ViewModels;

/// <summary>
/// メイン画面のViewModel
///
/// カード一覧の読み込み・選択、選択中カードの元画像表示、
/// ①～④を合成したプレビュー表示を担う。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly AppSettingsService _settingsService;
    private readonly CardCompositionService _compositionService;
    private readonly OcrService _ocrService;
    private readonly BitmapImage _frameTemplate;
    private bool _settingsDirty;

    // ②タイトル・④説明文の統一レイアウト設定。互いに別に保持する。
    // 編集用のUIはまだ無いため、起動時に読み込んだ値をそのまま使う。
    private readonly string _titleFontFamilyName;
    private readonly double _titleFontSize;
    private readonly double _titleLetterSpacing;
    private readonly string _descriptionFontFamilyName;
    private readonly double _descriptionFontSize;
    private readonly double _descriptionLetterSpacing;
    private readonly double _descriptionLineSpacing;

    [ObservableProperty]
    private IReadOnlyList<CardImage> _cardList;

    [ObservableProperty]
    private CardImage? _selectedCard;

    // 選択中カードの元画像。②③④の読取ボタンは、この画像の固定矩形領域から抽出する。
    [ObservableProperty]
    private BitmapImage? _sourceImage;

    // ①～④を合成したプレビュー画像。③④の描画が実装されるまでは①外枠・②タイトルのみ。
    [ObservableProperty]
    private BitmapSource? _previewImage;

    // ②タイトルの文字列。テキストボックスと双方向バインドし、変更のたびにプレビューを再合成する。
    [ObservableProperty]
    private string _titleText = string.Empty;

    // ③イラストの切り抜き画像。読取ボタンを押したときのみ更新する。
    [ObservableProperty]
    private BitmapSource? _illustrationImage;

    // ③イラストの縦横比キープ有無。初期値はキープしない。
    [ObservableProperty]
    private bool _keepIllustrationAspectRatio;

    // ④説明文の文字列。複数行はテキストボックスの改行(\n)で区切る。
    [ObservableProperty]
    private string _descriptionText = string.Empty;

    [ObservableProperty]
    private double _windowWidth;

    [ObservableProperty]
    private double _windowHeight;

    public MainViewModel(CardFolderService cardFolderService, CardCompositionService compositionService,
        OcrService ocrService, AppSettingsService settingsService, AppSettings settings, string cardFolder)
    {
        _settingsService = settingsService;
        _compositionService = compositionService;
        _ocrService = ocrService;

        _titleFontFamilyName = settings.TitleFontFamily;
        _titleFontSize = settings.TitleFontSize;
        _titleLetterSpacing = settings.TitleLetterSpacing;
        _descriptionFontFamilyName = settings.DescriptionFontFamily;
        _descriptionFontSize = settings.DescriptionFontSize;
        _descriptionLetterSpacing = settings.DescriptionLetterSpacing;
        _descriptionLineSpacing = settings.DescriptionLineSpacing;

        // 初期表示時点ではまだ設定変更ではないため、_settingsDirtyを立てないよう
        // プロパティではなくフィールドへ直接代入する。
        _windowWidth = settings.WindowWidth;
        _windowHeight = settings.WindowHeight;

        // OnSelectedCardChangedがRecomposePreviewを呼ぶ可能性があるため、
        // カード一覧の読み込みより先にフレームテンプレートを読み込んでおく。
        _frameTemplate = _compositionService.LoadFrameTemplate();

        _cardList = cardFolderService.LoadCardList(cardFolder);
        SelectedCard = CardList.FirstOrDefault();

        RecomposePreview();
    }

    /// <summary>
    /// タイトル領域からWindows OCRで文字列を抽出し、テキストボックスへ反映する
    /// </summary>
    [RelayCommand]
    private async Task ReadTitleAsync()
    {
        if (SourceImage is null)
            return;

        var cropped = ImageCropper.Crop(SourceImage, CardTemplateLayout.TitleRect);
        var recognized = await _ocrService.RecognizeTextAsync(cropped);

        // 元画像は文字ごとに間隔を空けて描画されている場合があるため、
        // 見た目上の空白は取り除き、文字間隔はアプリ側の統一設定で付け直す。
        TitleText = string.Concat(recognized.Where(c => !char.IsWhiteSpace(c)));
    }

    /// <summary>
    /// イラスト領域を矩形で切り抜き、表示矩形に貼り付ける
    /// </summary>
    [RelayCommand]
    private void ReadIllustration()
    {
        if (SourceImage is null)
            return;

        IllustrationImage = ImageCropper.Crop(SourceImage, CardTemplateLayout.IllustrationRect);
    }

    /// <summary>
    /// 説明文領域からWindows OCRで文字列を抽出し、テキストボックスへ反映する
    ///
    /// ②タイトルと同じ抽出処理を、複数行対応のため行単位で適用する。
    /// </summary>
    [RelayCommand]
    private async Task ReadDescriptionAsync()
    {
        if (SourceImage is null)
            return;

        var cropped = ImageCropper.Crop(SourceImage, CardTemplateLayout.DescriptionRect);
        var recognized = await _ocrService.RecognizeTextAsync(cropped);

        var lines = recognized.Split('\n').Select(line => string.Concat(line.Where(c => !char.IsWhiteSpace(c))));
        DescriptionText = string.Join('\n', lines);
    }

    partial void OnSelectedCardChanged(CardImage? value)
    {
        SourceImage = value is null ? null : LoadImage(value.FilePath);
        TitleText = string.Empty;
        IllustrationImage = null;
        DescriptionText = string.Empty;
    }

    partial void OnTitleTextChanged(string value) => RecomposePreview();

    partial void OnIllustrationImageChanged(BitmapSource? value) => RecomposePreview();

    partial void OnKeepIllustrationAspectRatioChanged(bool value) => RecomposePreview();

    partial void OnDescriptionTextChanged(string value) => RecomposePreview();

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
        _settingsService.Save(new AppSettings(WindowWidth, WindowHeight, _titleFontFamilyName, _titleFontSize, _titleLetterSpacing,
            _descriptionFontFamilyName, _descriptionFontSize, _descriptionLetterSpacing, _descriptionLineSpacing));
    }

    private void RecomposePreview()
    {
        var request = new CardCompositionRequest
        {
            FrameTemplate = _frameTemplate,
            TitleText = TitleText,
            TitleFontFamily = new FontFamily(_titleFontFamilyName),
            TitleFontSize = _titleFontSize,
            TitleLetterSpacing = _titleLetterSpacing,
            IllustrationImage = IllustrationImage,
            KeepIllustrationAspectRatio = KeepIllustrationAspectRatio,
            DescriptionText = DescriptionText,
            DescriptionFontFamily = new FontFamily(_descriptionFontFamilyName),
            DescriptionFontSize = _descriptionFontSize,
            DescriptionLetterSpacing = _descriptionLetterSpacing,
            DescriptionLineSpacing = _descriptionLineSpacing,
        };

        PreviewImage = _compositionService.Compose(request);
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
