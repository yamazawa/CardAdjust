using Microsoft.Win32;

namespace CardAdjust.Services;

/// <summary>
/// 保存先ファイルパスを選択するダイアログを表示するサービス
/// </summary>
public class SaveFileDialogService
{
    public string? ShowSavePngDialog(string defaultFileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "PNG画像 (*.png)|*.png",
            FileName = defaultFileName,
            DefaultExt = ".png",
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
