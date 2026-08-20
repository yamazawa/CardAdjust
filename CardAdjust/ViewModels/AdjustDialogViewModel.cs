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
    public bool IsCommonSetting { get; }

    public string TargetLabel { get; }

    public bool ShowLineSpacing { get; }

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

    public bool WasCanceled { get; private set; }

    public event Action? RequestClose;

    public event Action? LiveChanged;

    public event Action? ClearRequested;

    public AdjustDialogViewModel(string fontFamilyName, double fontSize, double letterSpacing, bool isBold, double lineSpacing,
        bool showLineSpacing, bool isCommonSetting, string targetLabel)
    {
        _fontFamilyName = fontFamilyName;
        _fontSize = fontSize;
        _letterSpacing = letterSpacing;
        _isBold = isBold;
        _lineSpacing = lineSpacing;
        ShowLineSpacing = showLineSpacing;
        IsCommonSetting = isCommonSetting;
        TargetLabel = targetLabel;
    }

    partial void OnFontFamilyNameChanged(string value) => LiveChanged?.Invoke();

    partial void OnFontSizeChanged(double value) => LiveChanged?.Invoke();

    partial void OnLetterSpacingChanged(double value) => LiveChanged?.Invoke();

    partial void OnIsBoldChanged(bool value) => LiveChanged?.Invoke();

    partial void OnLineSpacingChanged(double value) => LiveChanged?.Invoke();

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
