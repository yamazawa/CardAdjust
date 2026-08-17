using System.Windows;
using CardAdjust.ViewModels;
using CardAdjust.Views;

namespace CardAdjust.Services;

/// <summary>
/// 個別調整ダイアログを表示するサービス
/// </summary>
public class AdjustDialogService
{
    public AdjustDialogResult? Show(string fontFamilyName, double fontSize, double letterSpacing, double lineSpacing, bool showLineSpacing)
    {
        var viewModel = new AdjustDialogViewModel(fontFamilyName, fontSize, letterSpacing, lineSpacing, showLineSpacing);
        var dialog = new AdjustDialog(viewModel) { Owner = Application.Current.MainWindow };
        dialog.ShowDialog();
        return viewModel.Result;
    }
}
