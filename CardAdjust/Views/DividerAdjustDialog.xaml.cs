using System.Windows;
using CardAdjust.ViewModels;

namespace CardAdjust.Views;

public partial class DividerAdjustDialog : Window
{
    public DividerAdjustDialog(DividerAdjustDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += Close;
    }
}
