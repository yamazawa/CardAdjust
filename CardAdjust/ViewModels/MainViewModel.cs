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
    private readonly AdjustDialogService _adjustDialogService;
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

    // 個別調整ダイアログで設定した、文字区間ごとのフォント上書き。カード切替時にクリアする。
    private readonly List<CharacterStyleOverride> _titleOverrides = [];
    private readonly List<CharacterStyleOverride> _descriptionOverrides = [];

    // ④説明文の行間隔は文字区間ではなくテキスト全体への上書きのため、単独で持つ。
    private double? _descriptionLineSpacingOverride;

    [ObservableProperty]
    private IReadOnlyList<CardImage> _cardList;

    [ObservableProperty]
    private CardImage? _selectedCard;

    // 選択中カードの元画像。②③④の読取ボタンは、この画像の固定矩形領域から抽出する。
    [ObservableProperty]
    private BitmapImage? _sourceImage;

    // ①～④を合成したプレビュー画像。
    [ObservableProperty]
    private BitmapSource? _previewImage;

    // ②タイトルの文字列。テキストボックスと双方向バインドし、変更のたびにプレビューを再合成する。
    [ObservableProperty]
    private string _titleText = string.Empty;

    // ②タイトルのテキストボックスでの選択範囲。個別調整ダイアログの対象区間として使う。
    [ObservableProperty]
    private int _titleSelectionStart;

    [ObservableProperty]
    private int _titleSelectionLength;

    // ③イラストの切り抜き画像。読取ボタンを押したときのみ更新する。
    [ObservableProperty]
    private BitmapSource? _illustrationImage;

    // ③イラストの縦横比キープ有無。初期値はキープしない。
    [ObservableProperty]
    private bool _keepIllustrationAspectRatio;

    // ④説明文の文字列。複数行はテキストボックスの改行(\n)で区切る。
    [ObservableProperty]
    private string _descriptionText = string.Empty;

    // ④説明文のテキストボックスでの選択範囲。個別調整ダイアログの対象区間として使う。
    [ObservableProperty]
    private int _descriptionSelectionStart;

    [ObservableProperty]
    private int _descriptionSelectionLength;

    [ObservableProperty]
    private double _windowWidth;

    [ObservableProperty]
    private double _windowHeight;

    public MainViewModel(CardFolderService cardFolderService, CardCompositionService compositionService,
        OcrService ocrService, AdjustDialogService adjustDialogService, AppSettingsService settingsService,
        AppSettings settings, string cardFolder)
    {
        _settingsService = settingsService;
        _compositionService = compositionService;
        _ocrService = ocrService;
        _adjustDialogService = adjustDialogService;

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
    /// 選択中のタイトル文字列に対する個別調整ダイアログを開く
    /// </summary>
    [RelayCommand]
    private void AdjustTitle()
    {
        if (TitleSelectionLength <= 0)
            return;

        var existing = _titleOverrides.FirstOrDefault(o => o.Start == TitleSelectionStart && o.Length == TitleSelectionLength);
        var result = _adjustDialogService.Show(
            existing?.FontFamilyName ?? _titleFontFamilyName,
            existing?.FontSize ?? _titleFontSize,
            existing?.LetterSpacing ?? _titleLetterSpacing,
            lineSpacing: 0,
            showLineSpacing: false);

        if (result is null)
            return;

        _titleOverrides.RemoveAll(o => RangesOverlap(o, TitleSelectionStart, TitleSelectionLength));
        if (!result.IsCleared)
            _titleOverrides.Add(new CharacterStyleOverride(TitleSelectionStart, TitleSelectionLength, result.FontFamilyName, result.FontSize, result.LetterSpacing));

        RecomposePreview();
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

    /// <summary>
    /// 選択中の説明文文字列に対する個別調整ダイアログを開く(行間隔も対象に含める)
    /// </summary>
    [RelayCommand]
    private void AdjustDescription()
    {
        if (DescriptionSelectionLength <= 0)
            return;

        var existing = _descriptionOverrides.FirstOrDefault(o => o.Start == DescriptionSelectionStart && o.Length == DescriptionSelectionLength);
        var result = _adjustDialogService.Show(
            existing?.FontFamilyName ?? _descriptionFontFamilyName,
            existing?.FontSize ?? _descriptionFontSize,
            existing?.LetterSpacing ?? _descriptionLetterSpacing,
            _descriptionLineSpacingOverride ?? _descriptionLineSpacing,
            showLineSpacing: true);

        if (result is null)
            return;

        _descriptionOverrides.RemoveAll(o => RangesOverlap(o, DescriptionSelectionStart, DescriptionSelectionLength));
        _descriptionLineSpacingOverride = result.IsCleared ? null : result.LineSpacing;
        if (!result.IsCleared)
        {
            _descriptionOverrides.Add(new CharacterStyleOverride(
                DescriptionSelectionStart, DescriptionSelectionLength, result.FontFamilyName, result.FontSize, result.LetterSpacing));
        }

        RecomposePreview();
    }

    private static bool RangesOverlap(CharacterStyleOverride o, int start, int length) =>
        o.Start < start + length && start < o.Start + o.Length;

    partial void OnSelectedCardChanged(CardImage? value)
    {
        SourceImage = value is null ? null : LoadImage(value.FilePath);
        TitleText = string.Empty;
        IllustrationImage = null;
        DescriptionText = string.Empty;
        _titleOverrides.Clear();
        _descriptionOverrides.Clear();
        _descriptionLineSpacingOverride = null;
    }

    partial void OnTitleTextChanged(string value) => RecomposePreview();

    partial void OnIllustrationImageChanged(BitmapSource? value) => RecomposePreview();

    partial void OnKeepIllustrationAspectRatioChanged(bool value) => RecomposePreview();

    partial void OnDescriptionTextChanged(string value)
    {
        // AcceptsReturn=TrueのTextBoxはEnter入力時に\r\nを挿入するため、
        // 個別調整の区間位置が\nのみの想定とずれないよう正規化する。
        if (value.Contains('\r'))
        {
            DescriptionText = value.Replace("\r\n", "\n").Replace('\r', '\n');
            return;
        }

        RecomposePreview();
    }

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
        var titleStyles = CharacterStyleBuilder.Build(TitleText, new FontFamily(_titleFontFamilyName), _titleFontSize, _titleLetterSpacing, _titleOverrides);
        var descriptionStyles = CharacterStyleBuilder.Build(DescriptionText, new FontFamily(_descriptionFontFamilyName),
            _descriptionFontSize, _descriptionLetterSpacing, _descriptionOverrides);

        var request = new CardCompositionRequest
        {
            FrameTemplate = _frameTemplate,
            TitleText = TitleText,
            TitleCharacterStyles = titleStyles,
            IllustrationImage = IllustrationImage,
            KeepIllustrationAspectRatio = KeepIllustrationAspectRatio,
            DescriptionText = DescriptionText,
            DescriptionCharacterStyles = descriptionStyles,
            DescriptionLineSpacing = _descriptionLineSpacingOverride ?? _descriptionLineSpacing,
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
