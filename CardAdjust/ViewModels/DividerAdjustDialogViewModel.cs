using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CardAdjust.ViewModels;

/// <summary>
/// 水平線の個別調整ダイアログのViewModel
///
/// 各項目はLostFocus(TextBox.Textの既定のUpdateSourceTrigger)のたびにLiveChangedを発火し、
/// 呼び出し側が即座にプレビューへ反映する。
/// OKは何もしない(既に反映済み)。キャンセル時は呼び出し側が元の値に戻す。
/// </summary>
public partial class DividerAdjustDialogViewModel : ObservableObject
{
    public bool ShowClearButton { get; }

    [ObservableProperty]
    private double _thickness;

    [ObservableProperty]
    private double _marginTop;

    [ObservableProperty]
    private double _marginBottom;

    public bool WasCanceled { get; private set; }

    public event Action? RequestClose;

    public event Action? LiveChanged;

    public event Action? ClearRequested;

    public DividerAdjustDialogViewModel(double thickness, double marginTop, double marginBottom, bool hasExistingOverride)
    {
        _thickness = thickness;
        _marginTop = marginTop;
        _marginBottom = marginBottom;
        ShowClearButton = hasExistingOverride;
    }

    partial void OnThicknessChanged(double value) => LiveChanged?.Invoke();

    partial void OnMarginTopChanged(double value) => LiveChanged?.Invoke();

    partial void OnMarginBottomChanged(double value) => LiveChanged?.Invoke();

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
