using System.Windows;
using System.Windows.Media.Imaging;

namespace CardAdjust.Models;

/// <summary>
/// CardCompositionService.Composeへ渡す入力パラメータ
///
/// 個別調整(CharacterStyleOverride)は呼び出し側で解決済みのCharacterStyleとして渡す。
/// ①～④の貼付先矩形(TitleRect等)は、個別設定が無ければCardTemplateLayoutの全体設定を渡す。
/// </summary>
public sealed class CardCompositionRequest
{
    public required BitmapImage FrameTemplate { get; init; }

    public string TitleText { get; init; } = string.Empty;
    public required IReadOnlyList<CharacterStyle> TitleCharacterStyles { get; init; }
    public IReadOnlyList<CharacterStyleOverride> TitleOverrides { get; init; } = [];
    public Rect TitleRect { get; init; } = CardTemplateLayout.TitleRect;

    public BitmapSource? IllustrationImage { get; init; }
    public bool KeepIllustrationAspectRatio { get; init; }
    public Rect IllustrationRect { get; init; } = CardTemplateLayout.IllustrationRect;

    public string DescriptionText { get; init; } = string.Empty;
    public required IReadOnlyList<CharacterStyle> DescriptionCharacterStyles { get; init; }
    public IReadOnlyList<CharacterStyleOverride> DescriptionOverrides { get; init; } = [];
    public double DescriptionLineSpacing { get; init; }
    public Rect DescriptionRect { get; init; } = CardTemplateLayout.DescriptionRect;
    public DividerStyle DefaultDividerStyle { get; init; } = DividerStyle.Default;
    public IReadOnlyList<DividerStyleOverride> DividerOverrides { get; init; } = [];
}
