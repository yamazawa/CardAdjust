using System.IO;
using System.Windows.Media.Imaging;

namespace CardAdjust.Services;

/// <summary>
/// 合成済みのカード画像をPNGとして保存するサービス
/// </summary>
public class ImageSaveService
{
    public void SaveAsPng(BitmapSource image, string filePath)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));

        using var stream = File.Create(filePath);
        encoder.Save(stream);
    }
}
