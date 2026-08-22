using System.Windows;
using CardAdjust.ViewModels;
using CardAdjust.Views;

namespace CardAdjust.Services;

/// <summary>
/// 水平線の個別調整ダイアログをモーダル表示するサービス
/// </summary>
public class DividerAdjustDialogService
{
    public void ShowModal(DividerAdjustDialogViewModel viewModel)
    {
        var dialog = new DividerAdjustDialog(viewModel) { Owner = Application.Current.MainWindow };
        dialog.ShowDialog();
    }
}
