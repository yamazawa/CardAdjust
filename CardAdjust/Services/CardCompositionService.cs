using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// カード要素(①外枠～④説明文)を合成してプレビュー画像を作るサービス
///
/// ①外枠・②タイトル・③イラスト・④説明文を合成する。
/// </summary>
public class CardCompositionService
{
    private const string FrameTemplateUri = "pack://application:,,,/image/黒枠線テンプレート.png";

    private readonly CardTextRenderer _textRenderer = new();

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

    public BitmapSource Compose(CardCompositionRequest request)
    {
        var canvasRect = new Rect(0, 0, CardTemplateLayout.TemplateWidth, CardTemplateLayout.TemplateHeight);

        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            context.DrawImage(request.FrameTemplate, canvasRect);

            DrawIllustration(context, request.IllustrationImage, request.IllustrationRect, request.KeepIllustrationAspectRatio);

            _textRenderer.DrawCenteredSingleLine(context, request.TitleText, request.TitleCharacterStyles,
                request.TitleRect, Brushes.White, request.HighlightOverrides, request.TitleOverrides);

            _textRenderer.DrawLeftAlignedMultiLine(context, request.DescriptionText, request.DescriptionCharacterStyles,
                request.DescriptionRect, request.DescriptionLineSpacing, Brushes.White, request.HighlightOverrides, request.DescriptionOverrides);
        }

        var bitmap = new RenderTargetBitmap(
            (int)canvasRect.Width, (int)canvasRect.Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    // 縦横比をキープしない場合は表示矩形いっぱいに引き伸ばす。
    // キープする場合は矩形内に収まる最大サイズへ縮小し、余った側を中央揃えにする。
    private static void DrawIllustration(DrawingContext context, BitmapSource? image, Rect rect, bool keepAspectRatio)
    {
        if (image is null)
            return;

        if (!keepAspectRatio)
        {
            context.DrawImage(image, rect);
            return;
        }

        var scale = Math.Min(rect.Width / image.PixelWidth, rect.Height / image.PixelHeight);
        var width = image.PixelWidth * scale;
        var height = image.PixelHeight * scale;
        var x = rect.X + (rect.Width - width) / 2;
        var y = rect.Y + (rect.Height - height) / 2;

        context.DrawImage(image, new Rect(x, y, width, height));
    }
}
