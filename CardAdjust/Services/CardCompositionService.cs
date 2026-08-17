using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// カード要素(①外枠～④説明文)を合成してプレビュー画像を作るサービス
///
/// 現時点では①外枠のみを合成する。②③④の描画は各タスクで追加する。
/// </summary>
public class CardCompositionService
{
    private const string FrameTemplateUri = "pack://application:,,,/image/黒枠線テンプレート.png";

    public BitmapImage LoadFrameTemplate()
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(FrameTemplateUri);
        image.EndInit();
        image.Freeze();
        return image;
    }

    public BitmapSource Compose(BitmapSource frameTemplate)
    {
        var canvasRect = new Rect(0, 0, CardTemplateLayout.TemplateWidth, CardTemplateLayout.TemplateHeight);

        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.DrawImage(frameTemplate, canvasRect);
        }

        var bitmap = new RenderTargetBitmap(
            (int)canvasRect.Width, (int)canvasRect.Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }
}
