using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CardAdjust.ViewModels;

/// <summary>
/// 個別調整ダイアログのViewModel
///
/// OK/クリア/キャンセルの結果はResultで表す(nullはキャンセル)。
/// </summary>
public partial class AdjustDialogViewModel : ObservableObject
{
    public bool ShowLineSpacing { get; }

    [ObservableProperty]
    private string _fontFamilyName;

    [ObservableProperty]
    private double _fontSize;

    [ObservableProperty]
    private double _letterSpacing;

    [ObservableProperty]
    private double _lineSpacing;

    public AdjustDialogResult? Result { get; private set; }

    public event Action? RequestClose;

    public AdjustDialogViewModel(string fontFamilyName, double fontSize, double letterSpacing, double lineSpacing, bool showLineSpacing)
    {
        _fontFamilyName = fontFamilyName;
        _fontSize = fontSize;
        _letterSpacing = letterSpacing;
        _lineSpacing = lineSpacing;
        ShowLineSpacing = showLineSpacing;
    }

    [RelayCommand]
    private void Ok()
    {
        Result = new AdjustDialogResult(IsCleared: false, FontFamilyName, FontSize, LetterSpacing, LineSpacing);
        RequestClose?.Invoke();
    }

    [RelayCommand]
    private void Clear()
    {
        Result = new AdjustDialogResult(IsCleared: true, FontFamilyName, FontSize, LetterSpacing, LineSpacing);
        RequestClose?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        Result = null;
        RequestClose?.Invoke();
    }
}

/// <summary>
/// 個別調整ダイアログの結果
///
/// IsCleared時は、呼び出し側が対象区間の上書きを削除して統一設定に戻す。
/// </summary>
public sealed record AdjustDialogResult(bool IsCleared, string FontFamilyName, double FontSize, double LetterSpacing, double LineSpacing);
