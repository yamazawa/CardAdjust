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
    public IReadOnlyList<CharacterStyleOverride> TitleOverrides { get; init; } = [];

    public BitmapSource? IllustrationImage { get; init; }
    public bool KeepIllustrationAspectRatio { get; init; }

    public string DescriptionText { get; init; } = string.Empty;
    public required IReadOnlyList<CharacterStyle> DescriptionCharacterStyles { get; init; }
    public IReadOnlyList<CharacterStyleOverride> DescriptionOverrides { get; init; } = [];
    public double DescriptionLineSpacing { get; init; }

    // trueの場合、個別調整による上書きが適用されている文字を色分け表示し、
    // どの区間が何番の個別設定かを示す番号付きの矩形を重ねて描画する(プレビュー専用。保存・一斉出力ではfalseを渡す)。
    public bool HighlightOverrides { get; init; }
}
