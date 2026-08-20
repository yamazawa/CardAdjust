using System.Windows.Media.Imaging;

namespace CardAdjust.Services;

/// <summary>
/// カードの元画像ファイルをBitmapImageとして読み込むサービス
/// </summary>
public static class SourceImageLoader
{
    public static BitmapImage Load(string filePath)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(filePath);
        image.EndInit();
        image.Freeze();
        return image;
    }
}
