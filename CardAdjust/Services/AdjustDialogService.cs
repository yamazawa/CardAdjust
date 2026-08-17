using System.Windows;
using CardAdjust.ViewModels;
using CardAdjust.Views;

namespace CardAdjust.Services;

/// <summary>
/// 個別調整ダイアログをモーダル表示するサービス
///
/// 値の反映・取消は呼び出し側がAdjustDialogViewModelのイベント経由で行う。
/// </summary>
public class AdjustDialogService
{
    public void ShowModal(AdjustDialogViewModel viewModel)
    {
        var dialog = new AdjustDialog(viewModel) { Owner = Application.Current.MainWindow };
        dialog.ShowDialog();
    }
}
