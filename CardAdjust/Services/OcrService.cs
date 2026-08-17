using System.IO;
using System.Windows.Media.Imaging;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace CardAdjust.Services;

/// <summary>
/// Windows OCR(Windows.Media.Ocr)で画像から文字列を抽出するサービス
/// </summary>
public class OcrService
{
    public async Task<string> RecognizeTextAsync(BitmapSource image)
    {
        var softwareBitmap = await ToSoftwareBitmapAsync(image);
        var engine = OcrEngine.TryCreateFromLanguage(new Language("ja")) ?? OcrEngine.TryCreateFromUserProfileLanguages();
        if (engine is null)
            return string.Empty;

        var result = await engine.RecognizeAsync(softwareBitmap);
        return result.Text;
    }

    // WPFのBitmapSourceはWindows.Media.Ocrに直接渡せないため、
    // 一度PNGへエンコードしてからSoftwareBitmapへ変換する。
    private static async Task<SoftwareBitmap> ToSoftwareBitmapAsync(BitmapSource image)
    {
        using var memoryStream = new MemoryStream();
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
        encoder.Save(memoryStream);
        memoryStream.Position = 0;

        using var randomAccessStream = memoryStream.AsRandomAccessStream();
        var decoder = await global::Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(randomAccessStream);
        return await decoder.GetSoftwareBitmapAsync();
    }
}
