using System.Windows.Media.Imaging;

namespace CardAdjust.Models;

/// <summary>
/// CardCompositionService.Composeへ渡す入力パラメータ
///
/// 個別調整(CharacterStyleOverride)は呼び出し側で解決済みのCharacterStyleとして渡す。
/// </summary>
public sealed class CardCompositionRequest
{
    public required BitmapImage FrameTemplate { get; init; }

    public string TitleText { get; init; } = string.Empty;
    public required IReadOnlyList<CharacterStyle> TitleCharacterStyles { get; init; }

    public BitmapSource? IllustrationImage { get; init; }
    public bool KeepIllustrationAspectRatio { get; init; }

    public string DescriptionText { get; init; } = string.Empty;
    public required IReadOnlyList<CharacterStyle> DescriptionCharacterStyles { get; init; }
    public double DescriptionLineSpacing { get; init; }
}
