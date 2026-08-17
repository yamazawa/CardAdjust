using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CardAdjust.Models;

/// <summary>
/// CardCompositionService.Composeへ渡す入力パラメータ
///
/// ②③④を実装するタスクで、対応する項目をここに追加していく。
/// </summary>
public sealed class CardCompositionRequest
{
    public required BitmapImage FrameTemplate { get; init; }

    public string TitleText { get; init; } = string.Empty;
    public required FontFamily TitleFontFamily { get; init; }
    public double TitleFontSize { get; init; }
    public double TitleLetterSpacing { get; init; }

    public BitmapSource? IllustrationImage { get; init; }
    public bool KeepIllustrationAspectRatio { get; init; }

    public string DescriptionText { get; init; } = string.Empty;
    public required FontFamily DescriptionFontFamily { get; init; }
    public double DescriptionFontSize { get; init; }
    public double DescriptionLetterSpacing { get; init; }
    public double DescriptionLineSpacing { get; init; }
}
