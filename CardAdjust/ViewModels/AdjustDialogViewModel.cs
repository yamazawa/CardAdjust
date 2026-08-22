using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CardAdjust.ViewModels;

/// <summary>
/// 個別調整ダイアログのViewModel
///
/// 各項目はLostFocus(TextBox.Textの既定のUpdateSourceTrigger)のたびにLiveChangedを発火し、
/// 呼び出し側が即座にプレビューへ反映する。
/// OKは何もしない(既に反映済み)。キャンセル時は呼び出し側が元の値に戻す。
/// </summary>
public partial class AdjustDialogViewModel : ObservableObject
{
    private readonly string _sourceText;

    public bool IsCommonSetting { get; }

    [ObservableProperty]
    private string _targetLabel;

    public bool ShowLineSpacing { get; }

    // 共通設定編集時は対象区間の概念が無いため、開始位置・文字数の変更欄は個別設定時のみ表示する。
    public bool ShowRange => !IsCommonSetting;

    public bool ShowClearButton => !IsCommonSetting;

    [ObservableProperty]
    private string _fontFamilyName;

    [ObservableProperty]
    private double _fontSize;

    [ObservableProperty]
    private double _letterSpacing;

    [ObservableProperty]
    private bool _isBold;

    [ObservableProperty]
    private double _lineSpacing;

    [ObservableProperty]
    private int _start;

    [ObservableProperty]
    private int _length;

    public bool WasCanceled { get; private set; }

    public event Action? RequestClose;

    public event Action? LiveChanged;

    public event Action? ClearRequested;

    public AdjustDialogViewModel(string sourceText, string fontFamilyName, double fontSize, double letterSpacing, bool isBold,
        double lineSpacing, bool showLineSpacing, bool isCommonSetting, int start, int length)
    {
        _sourceText = sourceText;
        _fontFamilyName = fontFamilyName;
        _fontSize = fontSize;
        _letterSpacing = letterSpacing;
        _isBold = isBold;
        _lineSpacing = lineSpacing;
        _start = start;
        _length = length;
        ShowLineSpacing = showLineSpacing;
        IsCommonSetting = isCommonSetting;
        _targetLabel = BuildTargetLabel(isCommonSetting, sourceText, start, length);
    }

    partial void OnFontFamilyNameChanged(string value) => LiveChanged?.Invoke();

    partial void OnFontSizeChanged(double value) => LiveChanged?.Invoke();

    partial void OnLetterSpacingChanged(double value) => LiveChanged?.Invoke();

    partial void OnIsBoldChanged(bool value) => LiveChanged?.Invoke();

    partial void OnLineSpacingChanged(double value) => LiveChanged?.Invoke();

    partial void OnStartChanged(int value)
    {
        TargetLabel = BuildTargetLabel(IsCommonSetting, _sourceText, Start, Length);
        LiveChanged?.Invoke();
    }

    partial void OnLengthChanged(int value)
    {
        TargetLabel = BuildTargetLabel(IsCommonSetting, _sourceText, Start, Length);
        LiveChanged?.Invoke();
    }

    // 個別設定の場合は反映対象の文言列(範囲変更にも追従)、共通設定の場合は「共通設定」と表示する。
    private static string BuildTargetLabel(bool isCommonSetting, string sourceText, int start, int length)
    {
        if (isCommonSetting)
            return Resources.Strings.Label_CommonSetting;

        var clampedStart = Math.Clamp(start, 0, sourceText.Length);
        var clampedLength = Math.Clamp(length, 0, sourceText.Length - clampedStart);
        return sourceText.Substring(clampedStart, clampedLength);
    }

    [RelayCommand]
    private void Ok() => RequestClose?.Invoke();

    [RelayCommand]
    private void Clear()
    {
        ClearRequested?.Invoke();
        RequestClose?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        WasCanceled = true;
        RequestClose?.Invoke();
    }
}
