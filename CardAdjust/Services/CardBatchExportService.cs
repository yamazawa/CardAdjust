using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CardAdjust.Models;

namespace CardAdjust.Services;

/// <summary>
/// カード一覧の全カードを一斉にプレビュー画像へ合成し、PNGとして出力するサービス
///
/// 各カードは保存済みレイアウト(CardLayoutService)があればそれを使い、
/// 無い場合は空のテキスト・イラスト無しで合成する。
/// 統一レイアウト設定(フォント種類等)は呼び出し時点の共通設定をそのまま使う。
/// </summary>
public class CardBatchExportService
{
    private readonly CardLayoutService _layoutService;
    private readonly CardCompositionService _compositionService;
    private readonly ImageSaveService _imageSaveService;

    public CardBatchExportService(CardLayoutService layoutService, CardCompositionService compositionService, ImageSaveService imageSaveService)
    {
        _layoutService = layoutService;
        _compositionService = compositionService;
        _imageSaveService = imageSaveService;
    }

    public void ExportAll(IReadOnlyList<CardImage> cards, string outputFolder, BitmapImage frameTemplate, Brush foreground,
        CommonTextStyle titleStyle, CommonTextStyle descriptionStyle, double descriptionLineSpacing)
    {
        Directory.CreateDirectory(outputFolder);

        foreach (var card in cards)
            ExportOne(card, outputFolder, frameTemplate, foreground, titleStyle, descriptionStyle, descriptionLineSpacing);
    }

    private void ExportOne(CardImage card, string outputFolder, BitmapImage frameTemplate, Brush foreground,
        CommonTextStyle titleStyle, CommonTextStyle descriptionStyle, double descriptionLineSpacing)
    {
        var layout = _layoutService.TryGet(card.FilePath);
        var request = BuildRequest(card, layout, frameTemplate, foreground, titleStyle, descriptionStyle, descriptionLineSpacing);
        var composed = _compositionService.Compose(request);
        _imageSaveService.SaveAsPng(composed, Path.Combine(outputFolder, $"{card.DisplayName}.png"));
    }

    private CardCompositionRequest BuildRequest(CardImage card, CardLayout? layout, BitmapImage frameTemplate, Brush foreground,
        CommonTextStyle titleStyle, CommonTextStyle descriptionStyle, double descriptionLineSpacing)
    {
        var titleText = layout?.TitleText ?? string.Empty;
        var descriptionText = layout?.DescriptionText ?? string.Empty;

        return new CardCompositionRequest
        {
            FrameTemplate = frameTemplate,
            Foreground = foreground,
            TitleText = titleText,
            TitleCharacterStyles = CharacterStyleBuilder.Build(titleText, new FontFamily(titleStyle.FontFamilyName),
                titleStyle.FontSize, titleStyle.LetterSpacing, titleStyle.IsBold, layout?.TitleOverrides ?? []),
            TitleRect = ResolveRect(layout?.TitleDestRegion, CardTemplateLayout.TitleRect),
            IllustrationImage = CropIllustration(card, layout),
            KeepIllustrationAspectRatio = layout?.KeepIllustrationAspectRatio ?? false,
            IllustrationRect = ResolveRect(layout?.IllustrationDestRegion, CardTemplateLayout.IllustrationRect),
            DescriptionText = descriptionText,
            DescriptionCharacterStyles = CharacterStyleBuilder.Build(descriptionText, new FontFamily(descriptionStyle.FontFamilyName),
                descriptionStyle.FontSize, descriptionStyle.LetterSpacing, descriptionStyle.IsBold, layout?.DescriptionOverrides ?? []),
            DescriptionLineSpacing = descriptionLineSpacing,
            DescriptionRect = ResolveRect(layout?.DescriptionDestRegion, CardTemplateLayout.DescriptionRect),
        };
    }

    // イラストは矩形のみ保存対象のため、出力のたびに元画像から切り抜き直す。
    // 個別設定(IllustrationRegion)があればそれを使い、無ければ全体設定(CardTemplateLayout)で切り抜く。
    private static BitmapSource CropIllustration(CardImage card, CardLayout? layout)
    {
        var region = layout?.IllustrationRegion
            ?? new RegionOverride(CardTemplateLayout.IllustrationX, CardTemplateLayout.IllustrationY,
                CardTemplateLayout.IllustrationWidth, CardTemplateLayout.IllustrationHeight);
        return ImageCropper.Crop(SourceImageLoader.Load(card.FilePath), new Rect(region.X, region.Y, region.Width, region.Height));
    }

    // 個別設定(貼付先矩形)があればそれを使い、無ければ全体設定(CardTemplateLayoutの固定矩形)を使う。
    private static Rect ResolveRect(RegionOverride? region, Rect fallback) =>
        region is null ? fallback : new Rect(region.X, region.Y, region.Width, region.Height);
}
