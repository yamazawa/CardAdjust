using System.IO;
using System.Windows;
using System.Windows.Threading;
using CardAdjust.Services;
using CardAdjust.ViewModels;

namespace CardAdjust;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // config.iniにカードフォルダの指定が無い場合の既定値。
    private static readonly string FallbackCardFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "オークションポーカー", "使用カード");

    private static readonly TimeSpan AutoSaveInterval = TimeSpan.FromSeconds(1);

    private MainViewModel? _viewModel;
    private AppSettingsService? _settingsService;
    private DispatcherTimer? _autoSaveTimer;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var cardFolderConfigService = new CardFolderConfigService();
        var cardFolder = cardFolderConfigService.LoadOrCreateDefault(FallbackCardFolder);

        _settingsService = new AppSettingsService();
        var appSettings = _settingsService.LoadOrCreateDefault();

        var cardFolderService = new CardFolderService();
        _viewModel = new MainViewModel(cardFolderService, _settingsService, appSettings, cardFolder);

        _autoSaveTimer = new DispatcherTimer { Interval = AutoSaveInterval };
        _autoSaveTimer.Tick += (_, _) => _viewModel.SaveSettingsIfDirty();
        _autoSaveTimer.Start();

        var mainWindow = new MainWindow { DataContext = _viewModel };
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _viewModel?.SaveSettingsIfDirty();
        base.OnExit(e);
    }
}
