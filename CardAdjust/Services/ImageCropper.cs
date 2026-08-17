using System.Windows;
using System.Windows.Media.Imaging;

namespace CardAdjust.Services;

/// <summary>
/// 画像から矩形領域を切り出すユーティリティ
/// </summary>
public static class ImageCropper
{
    public static BitmapSource Crop(BitmapSource source, Rect rect)
    {
        var x = (int)Math.Max(0, rect.X);
        var y = (int)Math.Max(0, rect.Y);
        var width = (int)Math.Min(rect.Width, source.PixelWidth - x);
        var height = (int)Math.Min(rect.Height, source.PixelHeight - y);

        return new CroppedBitmap(source, new Int32Rect(x, y, width, height));
    }
}
