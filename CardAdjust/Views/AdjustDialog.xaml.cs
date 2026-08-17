using System.Windows;
using CardAdjust.ViewModels;

namespace CardAdjust.Views;

public partial class AdjustDialog : Window
{
    public AdjustDialog(AdjustDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += Close;
    }
}
