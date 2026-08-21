using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CardAdjust.Models;
using CardAdjust.Resources;
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
    private readonly CardLayoutService _layoutService;
    private readonly CardCompositionService _compositionService;
    private readonly OcrService _ocrService;
    private readonly AdjustDialogService _adjustDialogService;
    private readonly SaveFileDialogService _saveFileDialogService;
    private readonly ImageSaveService _imageSaveService;
    private readonly CardBatchExportService _batchExportService;
    private readonly BitmapImage _frameTemplate;
    private bool _settingsDirty;
    private bool _layoutDirty;

    // カード切り替え時、直前のカードのレイアウトを保存するために保持する。
    private string? _previousCardFilePath;

    // カードの読込・復元中はユーザー操作ではないため、MarkLayoutDirtyを抑制するためのガード。
    private bool _isRestoringLayout;

    // 直前のテキスト。文字入力で増えた区間を検出し、個別設定(Overrides)を追従させるために保持する。
    private string _previousTitleText = string.Empty;
    private string _previousDescriptionText = string.Empty;

    // ②タイトル・④説明文の統一レイアウト設定＋文字区間ごとの個別上書き。互いに別に保持する。
    private readonly TextElementState _titleState;
    private readonly TextElementState _descriptionState;

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

    // ②タイトルの個別設定一覧。プレビュー上の番号付き矩形と同じ番号で対応する。
    [ObservableProperty]
    private IReadOnlyList<OverrideSummary> _titleOverrideSummaries = [];

    // ②タイトルの読取矩形。元画像上のドラッグ(位置)・リサイズ(幅高さ別々)で調整する(初期値はCardTemplateLayout)。
    [ObservableProperty]
    private double _titleRegionX = CardTemplateLayout.TitleX;

    [ObservableProperty]
    private double _titleRegionY = CardTemplateLayout.TitleY;

    [ObservableProperty]
    private double _titleRegionWidth = CardTemplateLayout.TitleWidth;

    [ObservableProperty]
    private double _titleRegionHeight = CardTemplateLayout.TitleHeight;

    // ③イラストの切り抜き画像。読取ボタンを押したときのみ更新する。
    [ObservableProperty]
    private BitmapSource? _illustrationImage;

    // ③イラストの縦横比キープ有無。初期値はキープしない。
    [ObservableProperty]
    private bool _keepIllustrationAspectRatio;

    // ③イラストの読取矩形。元画像上のドラッグ(位置)・リサイズ(幅高さ別々)で調整する(初期値はCardTemplateLayout)。
    [ObservableProperty]
    private double _illustrationRegionX = CardTemplateLayout.IllustrationX;

    [ObservableProperty]
    private double _illustrationRegionY = CardTemplateLayout.IllustrationY;

    [ObservableProperty]
    private double _illustrationRegionWidth = CardTemplateLayout.IllustrationWidth;

    [ObservableProperty]
    private double _illustrationRegionHeight = CardTemplateLayout.IllustrationHeight;

    // ④説明文の文字列。複数行はテキストボックスの改行(\n)で区切る。
    [ObservableProperty]
    private string _descriptionText = string.Empty;

    // ④説明文のテキストボックスでの選択範囲。個別調整ダイアログの対象区間として使う。
    [ObservableProperty]
    private int _descriptionSelectionStart;

    [ObservableProperty]
    private int _descriptionSelectionLength;

    // ④説明文の個別設定一覧。プレビュー上の番号付き矩形と同じ番号で対応する。
    [ObservableProperty]
    private IReadOnlyList<OverrideSummary> _descriptionOverrideSummaries = [];

    // ④説明文の読取矩形。元画像上のドラッグ(位置)・リサイズ(幅高さ別々)で調整する(初期値はCardTemplateLayout)。
    [ObservableProperty]
    private double _descriptionRegionX = CardTemplateLayout.DescriptionX;

    [ObservableProperty]
    private double _descriptionRegionY = CardTemplateLayout.DescriptionY;

    [ObservableProperty]
    private double _descriptionRegionWidth = CardTemplateLayout.DescriptionWidth;

    [ObservableProperty]
    private double _descriptionRegionHeight = CardTemplateLayout.DescriptionHeight;

    [ObservableProperty]
    private double _windowWidth;

    [ObservableProperty]
    private double _windowHeight;

    // 一斉出力の出力先フォルダパス。次回起動時も保持する。
    [ObservableProperty]
    private string _batchExportFolder;

    // 個別調整による上書きをプレビュー画面上で色分け表示するか。次回起動時も保持する。
    [ObservableProperty]
    private bool _highlightOverridesEnabled;

    public MainViewModel(CardFolderService cardFolderService, CardCompositionService compositionService,
        OcrService ocrService, AdjustDialogService adjustDialogService, SaveFileDialogService saveFileDialogService,
        ImageSaveService imageSaveService, AppSettingsService settingsService, CardLayoutService layoutService,
        CardBatchExportService batchExportService, AppSettings settings, string cardFolder)
    {
        _settingsService = settingsService;
        _layoutService = layoutService;
        _compositionService = compositionService;
        _ocrService = ocrService;
        _adjustDialogService = adjustDialogService;
        _saveFileDialogService = saveFileDialogService;
        _imageSaveService = imageSaveService;
        _batchExportService = batchExportService;

        _titleState = new TextElementState
        {
            FontFamilyName = settings.TitleFontFamily,
            FontSize = settings.TitleFontSize,
            LetterSpacing = settings.TitleLetterSpacing,
            IsBold = settings.TitleBold,
        };
        _descriptionState = new TextElementState
        {
            FontFamilyName = settings.DescriptionFontFamily,
            FontSize = settings.DescriptionFontSize,
            LetterSpacing = settings.DescriptionLetterSpacing,
            IsBold = settings.DescriptionBold,
            LineSpacing = settings.DescriptionLineSpacing,
        };

        // 初期表示時点ではまだ設定変更ではないため、_settingsDirtyを立てないよう
        // プロパティではなくフィールドへ直接代入する。
        _windowWidth = settings.WindowWidth;
        _windowHeight = settings.WindowHeight;
        _batchExportFolder = settings.BatchExportFolder;
        _highlightOverridesEnabled = settings.HighlightOverridesEnabled;

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

        var rect = new Rect(TitleRegionX, TitleRegionY, TitleRegionWidth, TitleRegionHeight);
        var cropped = ImageCropper.Crop(SourceImage, rect);
        var recognized = await _ocrService.RecognizeTextAsync(cropped);

        // 元画像は文字ごとに間隔を空けて描画されている場合があるため、
        // 見た目上の空白は取り除き、文字間隔はアプリ側の統一設定で付け直す。
        TitleText = string.Concat(recognized.Where(c => !char.IsWhiteSpace(c)));
    }

    /// <summary>
    /// タイトルの個別調整ダイアログを開く
    ///
    /// 範囲選択が無い場合は、②タイトルの共通設定を編集する。
    /// </summary>
    [RelayCommand]
    private void AdjustTitle() => OpenAdjustDialog(TitleText, TitleSelectionStart, TitleSelectionLength, _titleState, showLineSpacing: false);

    /// <summary>
    /// 個別設定一覧の番号から、対象のOverridesを直接編集する
    /// </summary>
    [RelayCommand]
    private void AdjustTitleOverride(OverrideSummary? summary)
    {
        if (summary is null)
            return;

        OpenAdjustDialog(TitleText, summary.Override.Start, summary.Override.Length, _titleState, showLineSpacing: false);
    }

    /// <summary>
    /// イラスト領域を矩形で切り抜き、表示矩形に貼り付ける
    /// </summary>
    [RelayCommand]
    private void ReadIllustration()
    {
        if (SourceImage is null)
            return;

        var rect = new Rect(IllustrationRegionX, IllustrationRegionY, IllustrationRegionWidth, IllustrationRegionHeight);
        IllustrationImage = ImageCropper.Crop(SourceImage, rect);
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

        var rect = new Rect(DescriptionRegionX, DescriptionRegionY, DescriptionRegionWidth, DescriptionRegionHeight);
        var cropped = ImageCropper.Crop(SourceImage, rect);
        var recognized = await _ocrService.RecognizeTextAsync(cropped);

        var lines = recognized.Split('\n').Select(line => string.Concat(line.Where(c => !char.IsWhiteSpace(c))));
        DescriptionText = string.Join('\n', lines);
    }

    /// <summary>
    /// 説明文の個別調整ダイアログを開く(行間隔も対象に含める)
    ///
    /// 範囲選択が無い場合は、④説明文の共通設定を編集する。
    /// </summary>
    [RelayCommand]
    private void AdjustDescription() =>
        OpenAdjustDialog(DescriptionText, DescriptionSelectionStart, DescriptionSelectionLength, _descriptionState, showLineSpacing: true);

    /// <summary>
    /// 個別設定一覧の番号から、対象のOverridesを直接編集する
    /// </summary>
    [RelayCommand]
    private void AdjustDescriptionOverride(OverrideSummary? summary)
    {
        if (summary is null)
            return;

        OpenAdjustDialog(DescriptionText, summary.Override.Start, summary.Override.Length, _descriptionState, showLineSpacing: true);
    }

    /// <summary>
    /// ①～④を合成した完成画像を保存先を選んでPNGとして保存する
    ///
    /// プレビュー画面の個別設定の色分け表示(HighlightOverrides)は、
    /// 編集用の表示のため保存画像には反映しない。
    /// </summary>
    [RelayCommand]
    private void SaveImage()
    {
        if (SelectedCard is null)
            return;

        var filePath = _saveFileDialogService.ShowSavePngDialog($"{SelectedCard.DisplayName}.png");
        if (filePath is null)
            return;

        var image = _compositionService.Compose(BuildCompositionRequest(highlightOverrides: false));
        _imageSaveService.SaveAsPng(image, filePath);
    }

    /// <summary>
    /// カード一覧の全カードのプレビュー画像を、出力先フォルダへ一斉にPNG保存する
    /// </summary>
    [RelayCommand]
    private void BatchExport()
    {
        if (string.IsNullOrWhiteSpace(BatchExportFolder))
            return;

        // 一斉出力の対象には選択中カードの未保存の変更も含めるため、先に確定させる。
        SaveLayoutIfDirty();

        var titleStyle = new CommonTextStyle(_titleState.FontFamilyName, _titleState.FontSize, _titleState.LetterSpacing, _titleState.IsBold);
        var descriptionStyle = new CommonTextStyle(_descriptionState.FontFamilyName, _descriptionState.FontSize, _descriptionState.LetterSpacing, _descriptionState.IsBold);
        _batchExportService.ExportAll(CardList, BatchExportFolder, _frameTemplate, titleStyle, descriptionStyle, _descriptionState.LineSpacing);
    }

    // 範囲選択が無い場合は共通設定(IsCommonSetting)、ある場合はその区間の個別上書きを編集する。
    // 各項目の変更はLostFocusのたびに即座にプレビューへ反映し(LiveChanged)、
    // キャンセル時はダイアログを開いた時点の状態に戻す。
    private void OpenAdjustDialog(string text, int selectionStart, int selectionLength, TextElementState state, bool showLineSpacing)
    {
        var isCommon = selectionLength <= 0;
        var start = isCommon ? 0 : selectionStart;
        var length = isCommon ? 0 : selectionLength;
        var existing = isCommon ? null : state.Overrides.FirstOrDefault(o => o.Start == start && o.Length == length);

        var viewModel = new AdjustDialogViewModel(text,
            existing?.FontFamilyName ?? state.FontFamilyName,
            existing?.FontSize ?? state.FontSize,
            existing?.LetterSpacing ?? state.LetterSpacing,
            existing?.IsBold ?? state.IsBold,
            state.LineSpacing,
            showLineSpacing, isCommon, start, length);

        var snapshotFontFamilyName = state.FontFamilyName;
        var snapshotFontSize = state.FontSize;
        var snapshotLetterSpacing = state.LetterSpacing;
        var snapshotIsBold = state.IsBold;
        var snapshotLineSpacing = state.LineSpacing;
        var snapshotOverrides = state.Overrides.ToList();

        // 個別設定はダイアログ内で開始位置・文字数(範囲)自体も変更できるため、
        // 「現在この区間に対応するOverridesの位置」を保持し、範囲変更のたびに追従させる。
        var range = new RangeTracker(start, length);

        viewModel.LiveChanged += () => ApplyAdjustLive(state, isCommon, range, viewModel);
        viewModel.ClearRequested += () => state.Overrides.RemoveAll(o => RangesOverlap(o, range.Start, range.Length));

        _adjustDialogService.ShowModal(viewModel);

        if (viewModel.WasCanceled)
            RevertAdjust(state, snapshotFontFamilyName, snapshotFontSize, snapshotLetterSpacing, snapshotIsBold, snapshotLineSpacing, snapshotOverrides);

        // 個別上書き(文字区間ごとのOverrides)はカードごとの保存対象のため、
        // 共通/個別いずれの変更でも(取り消し後の確定値を含めて)レイアウト保存をマークする。
        MarkLayoutDirty();
        RecomposePreview();
    }

    // 行間隔は文字区間の概念に馴染まないため、共通/個別どちらのモードでも常にstate全体へ反映する。
    private void ApplyAdjustLive(TextElementState state, bool isCommon, RangeTracker range, AdjustDialogViewModel viewModel)
    {
        state.LineSpacing = viewModel.LineSpacing;

        if (isCommon)
        {
            state.FontFamilyName = viewModel.FontFamilyName;
            state.FontSize = viewModel.FontSize;
            state.LetterSpacing = viewModel.LetterSpacing;
            state.IsBold = viewModel.IsBold;
        }
        else
        {
            // 直前の位置にあったOverridesを取り除いてから、ダイアログ側の最新の開始位置・文字数で追加し直す。
            state.Overrides.RemoveAll(o => RangesOverlap(o, range.Start, range.Length));
            range.Start = viewModel.Start;
            range.Length = viewModel.Length;
            state.Overrides.Add(new CharacterStyleOverride(range.Start, range.Length, viewModel.FontFamilyName, viewModel.FontSize, viewModel.LetterSpacing, viewModel.IsBold));
        }

        MarkSettingsDirty();
        RecomposePreview();
    }

    private static void RevertAdjust(TextElementState state, string fontFamilyName, double fontSize, double letterSpacing,
        bool isBold, double lineSpacing, List<CharacterStyleOverride> overrides)
    {
        state.FontFamilyName = fontFamilyName;
        state.FontSize = fontSize;
        state.LetterSpacing = letterSpacing;
        state.IsBold = isBold;
        state.LineSpacing = lineSpacing;
        state.Overrides.Clear();
        state.Overrides.AddRange(overrides);
    }

    private static bool RangesOverlap(CharacterStyleOverride o, int start, int length) =>
        o.Start < start + length && start < o.Start + o.Length;

    partial void OnSelectedCardChanged(CardImage? value)
    {
        // 直前のカードの未保存の変更を、切り替え前に確定させる。
        SaveLayoutIfDirty(_previousCardFilePath);
        _previousCardFilePath = value?.FilePath;

        SourceImage = value is null ? null : SourceImageLoader.Load(value.FilePath);
        var layout = value is null ? null : _layoutService.TryGet(value.FilePath);

        // 復元自体はユーザー操作による変更ではないため、MarkLayoutDirtyを抑制する。
        _isRestoringLayout = true;
        RestoreTextAndAspect(layout);
        RestoreRegions(layout);
        RestoreOverrides(layout);
        _isRestoringLayout = false;

        // イラストは矩形のみ保存対象のため、保存済みレイアウトがある場合のみ元画像から再取得する。
        IllustrationImage = layout is not null && SourceImage is not null
            ? ImageCropper.Crop(SourceImage, new Rect(IllustrationRegionX, IllustrationRegionY, IllustrationRegionWidth, IllustrationRegionHeight))
            : null;

        RecomposePreview();
    }

    private void RestoreTextAndAspect(CardLayout? layout)
    {
        TitleText = layout?.TitleText ?? string.Empty;
        DescriptionText = layout?.DescriptionText ?? string.Empty;
        KeepIllustrationAspectRatio = layout?.KeepIllustrationAspectRatio ?? false;
    }

    private void RestoreRegions(CardLayout? layout)
    {
        TitleRegionX = layout?.TitleRegionX ?? CardTemplateLayout.TitleX;
        TitleRegionY = layout?.TitleRegionY ?? CardTemplateLayout.TitleY;
        TitleRegionWidth = layout?.TitleRegionWidth ?? CardTemplateLayout.TitleWidth;
        TitleRegionHeight = layout?.TitleRegionHeight ?? CardTemplateLayout.TitleHeight;
        IllustrationRegionX = layout?.IllustrationRegionX ?? CardTemplateLayout.IllustrationX;
        IllustrationRegionY = layout?.IllustrationRegionY ?? CardTemplateLayout.IllustrationY;
        IllustrationRegionWidth = layout?.IllustrationRegionWidth ?? CardTemplateLayout.IllustrationWidth;
        IllustrationRegionHeight = layout?.IllustrationRegionHeight ?? CardTemplateLayout.IllustrationHeight;
        DescriptionRegionX = layout?.DescriptionRegionX ?? CardTemplateLayout.DescriptionX;
        DescriptionRegionY = layout?.DescriptionRegionY ?? CardTemplateLayout.DescriptionY;
        DescriptionRegionWidth = layout?.DescriptionRegionWidth ?? CardTemplateLayout.DescriptionWidth;
        DescriptionRegionHeight = layout?.DescriptionRegionHeight ?? CardTemplateLayout.DescriptionHeight;
    }

    // 文字区間ごとの個別上書きも、SP2からはカードごとの保存対象になる。
    // 共通設定(フォント・サイズ・間隔)はアプリ全体の設定なので維持する。
    private void RestoreOverrides(CardLayout? layout)
    {
        _titleState.Overrides.Clear();
        _titleState.Overrides.AddRange(layout?.TitleOverrides ?? []);
        _descriptionState.Overrides.Clear();
        _descriptionState.Overrides.AddRange(layout?.DescriptionOverrides ?? []);
    }

    partial void OnTitleTextChanged(string value)
    {
        if (!_isRestoringLayout)
            ShiftOverridesForInsertion(_titleState.Overrides, _previousTitleText, value);
        _previousTitleText = value;

        MarkLayoutDirty();
        RecomposePreview();
    }

    partial void OnIllustrationImageChanged(BitmapSource? value) => RecomposePreview();

    partial void OnKeepIllustrationAspectRatioChanged(bool value)
    {
        MarkLayoutDirty();
        RecomposePreview();
    }

    partial void OnDescriptionTextChanged(string value)
    {
        MarkLayoutDirty();

        // AcceptsReturn=TrueのTextBoxはEnter入力時に\r\nを挿入するため、
        // 個別調整の区間位置が\nのみの想定とずれないよう正規化する。
        // (正規化前の\r\n混じりの値ではOverridesを追従させない。正規化後に1回だけ行う。)
        if (value.Contains('\r'))
        {
            DescriptionText = value.Replace("\r\n", "\n").Replace('\r', '\n');
            return;
        }

        if (!_isRestoringLayout)
            ShiftOverridesForInsertion(_descriptionState.Overrides, _previousDescriptionText, value);
        _previousDescriptionText = value;

        RecomposePreview();
    }

    // 文字入力で文字数が増えた場合、増えた区間の位置(insertIndex)に応じて個別設定(Overrides)を追従させる。
    // 増えた区間を含む個別設定は文字数を増やし、それより開始位置が後ろの個別設定は開始位置をずらす。
    // (文字数が減った場合は対象外。個別調整ダイアログの開始位置・文字数欄で手動修正できる。)
    private static void ShiftOverridesForInsertion(List<CharacterStyleOverride> overrides, string oldText, string newText)
    {
        if (newText.Length <= oldText.Length)
            return;

        var insertIndex = CommonPrefixLength(oldText, newText);
        var insertedCount = newText.Length - oldText.Length;

        for (var i = 0; i < overrides.Count; i++)
        {
            var o = overrides[i];
            if (insertIndex >= o.Start && insertIndex < o.Start + o.Length)
                overrides[i] = o with { Length = o.Length + insertedCount };
            else if (insertIndex < o.Start)
                overrides[i] = o with { Start = o.Start + insertedCount };
        }
    }

    private static int CommonPrefixLength(string a, string b)
    {
        var max = Math.Min(a.Length, b.Length);
        var i = 0;
        while (i < max && a[i] == b[i])
            i++;

        return i;
    }

    partial void OnTitleRegionXChanged(double value) => MarkLayoutDirty();

    partial void OnTitleRegionYChanged(double value) => MarkLayoutDirty();

    partial void OnTitleRegionWidthChanged(double value) => MarkLayoutDirty();

    partial void OnTitleRegionHeightChanged(double value) => MarkLayoutDirty();

    partial void OnIllustrationRegionXChanged(double value) => MarkLayoutDirty();

    partial void OnIllustrationRegionYChanged(double value) => MarkLayoutDirty();

    partial void OnIllustrationRegionWidthChanged(double value) => MarkLayoutDirty();

    partial void OnIllustrationRegionHeightChanged(double value) => MarkLayoutDirty();

    partial void OnDescriptionRegionXChanged(double value) => MarkLayoutDirty();

    partial void OnDescriptionRegionYChanged(double value) => MarkLayoutDirty();

    partial void OnDescriptionRegionWidthChanged(double value) => MarkLayoutDirty();

    partial void OnDescriptionRegionHeightChanged(double value) => MarkLayoutDirty();

    partial void OnWindowWidthChanged(double value) => MarkSettingsDirty();

    partial void OnWindowHeightChanged(double value) => MarkSettingsDirty();

    partial void OnBatchExportFolderChanged(string value) => MarkSettingsDirty();

    partial void OnHighlightOverridesEnabledChanged(bool value)
    {
        MarkSettingsDirty();
        RecomposePreview();
    }

    private void MarkSettingsDirty() => _settingsDirty = true;

    private void MarkLayoutDirty()
    {
        if (!_isRestoringLayout)
            _layoutDirty = true;
    }

    /// <summary>
    /// ウィンドウサイズ・フォント設定に変更があれば設定を保存する
    /// </summary>
    public void SaveSettingsIfDirty()
    {
        if (!_settingsDirty)
            return;

        _settingsDirty = false;
        _settingsService.Save(new AppSettings(WindowWidth, WindowHeight,
            _titleState.FontFamilyName, _titleState.FontSize, _titleState.LetterSpacing, _titleState.IsBold,
            _descriptionState.FontFamilyName, _descriptionState.FontSize, _descriptionState.LetterSpacing, _descriptionState.LineSpacing, _descriptionState.IsBold,
            BatchExportFolder, HighlightOverridesEnabled));
    }

    /// <summary>
    /// 選択中カードのレイアウトに変更があれば保存する
    /// </summary>
    public void SaveLayoutIfDirty() => SaveLayoutIfDirty(SelectedCard?.FilePath);

    private void SaveLayoutIfDirty(string? filePath)
    {
        if (!_layoutDirty || filePath is null)
            return;

        _layoutDirty = false;
        _layoutService.Save(filePath, BuildCurrentLayout());
    }

    private CardLayout BuildCurrentLayout() => new(
        TitleRegionX, TitleRegionY, TitleRegionWidth, TitleRegionHeight,
        IllustrationRegionX, IllustrationRegionY, IllustrationRegionWidth, IllustrationRegionHeight,
        DescriptionRegionX, DescriptionRegionY, DescriptionRegionWidth, DescriptionRegionHeight,
        TitleText, DescriptionText, KeepIllustrationAspectRatio,
        _titleState.Overrides.ToList(), _descriptionState.Overrides.ToList());

    // プレビューではHighlightOverridesEnabledに応じて個別調整による上書きを色分け表示する。
    // 保存・一斉出力では色分け表示しない(SaveImage側でhighlightOverrides: falseで再合成する)。
    private void RecomposePreview()
    {
        RefreshOverrideSummaries();
        PreviewImage = _compositionService.Compose(BuildCompositionRequest(HighlightOverridesEnabled));
    }

    private CardCompositionRequest BuildCompositionRequest(bool highlightOverrides)
    {
        var titleStyles = CharacterStyleBuilder.Build(TitleText, new FontFamily(_titleState.FontFamilyName),
            _titleState.FontSize, _titleState.LetterSpacing, _titleState.IsBold, _titleState.Overrides);
        var descriptionStyles = CharacterStyleBuilder.Build(DescriptionText, new FontFamily(_descriptionState.FontFamilyName),
            _descriptionState.FontSize, _descriptionState.LetterSpacing, _descriptionState.IsBold, _descriptionState.Overrides);

        return new CardCompositionRequest
        {
            FrameTemplate = _frameTemplate,
            TitleText = TitleText,
            TitleCharacterStyles = titleStyles,
            TitleOverrides = _titleState.Overrides,
            IllustrationImage = IllustrationImage,
            KeepIllustrationAspectRatio = KeepIllustrationAspectRatio,
            DescriptionText = DescriptionText,
            DescriptionCharacterStyles = descriptionStyles,
            DescriptionOverrides = _descriptionState.Overrides,
            DescriptionLineSpacing = _descriptionState.LineSpacing,
            HighlightOverrides = highlightOverrides,
        };
    }

    // 個別設定一覧(番号付き)を、プレビュー上の番号付き矩形と同じ順序(開始位置順)で組み立てる。
    private void RefreshOverrideSummaries()
    {
        TitleOverrideSummaries = BuildOverrideSummaries(TitleText, _titleState.Overrides);
        DescriptionOverrideSummaries = BuildOverrideSummaries(DescriptionText, _descriptionState.Overrides);
    }

    private static IReadOnlyList<OverrideSummary> BuildOverrideSummaries(string text, IReadOnlyList<CharacterStyleOverride> overrides) =>
        overrides.OrderBy(o => o.Start)
            .Select((o, i) => new OverrideSummary(i + 1, BuildOverrideSummaryLabel(i + 1, text, o), o))
            .ToList();

    private static string BuildOverrideSummaryLabel(int number, string text, CharacterStyleOverride o)
    {
        var start = Math.Clamp(o.Start, 0, text.Length);
        var length = Math.Clamp(o.Length, 0, text.Length - start);
        return $"{number}: {text.Substring(start, length)}";
    }

    // ②タイトル・④説明文それぞれの、統一設定(共通設定)と文字区間ごとの個別上書きを保持する。
    private sealed class TextElementState
    {
        public required string FontFamilyName { get; set; }
        public required double FontSize { get; set; }
        public required double LetterSpacing { get; set; }
        public bool IsBold { get; set; }
        public double LineSpacing { get; set; }
        public List<CharacterStyleOverride> Overrides { get; } = [];
    }

    // 個別調整ダイアログを開いている間、対象のOverridesが現在どの区間にあるかを追跡する。
    // ダイアログ側で開始位置・文字数(範囲)自体が変更された場合、この位置も追従させる。
    private sealed class RangeTracker(int start, int length)
    {
        public int Start { get; set; } = start;
        public int Length { get; set; } = length;
    }
}
